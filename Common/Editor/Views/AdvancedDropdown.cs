using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Editor.Tree;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MisterGames.Common.Editor.Views {

	public sealed class AdvancedDropdown<T> : AdvancedDropdown {

		private readonly string _title;
		private readonly Action<T> _onItemSelected;
		private readonly Func<IEnumerable<TreeEntry<PathTree.Node<T>>>, IEnumerable<TreeEntry<PathTree.Node<T>>>> _sort;
		private readonly TreeEntry<PathTree.Node<T>> _pathTreeRoot;

		private sealed class Item : AdvancedDropdownItem {

			public readonly T data;

			public Item(T data, string name) : base(name) {
				this.data = data;
			}
		}

		public AdvancedDropdown(
			string title,
			IEnumerable<T> items,
			Func<T, string> getItemPath,
			Action<T> onItemSelected,
			char separator = '/',
			Func<IEnumerable<TreeEntry<PathTree.Node<T>>>, IEnumerable<TreeEntry<PathTree.Node<T>>>> sort = null
		) : base(new AdvancedDropdownState()) {

			_title = title;
			_onItemSelected = onItemSelected;
			_pathTreeRoot = PathTree.CreateTree(items, getItemPath, separator, sort);

			float width = Mathf.Max(minimumSize.x, 240f);
			float height = 14 * EditorGUIUtility.singleLineHeight;

			minimumSize = new Vector2(width, height);
		}

		protected override AdvancedDropdownItem BuildRoot() {
			var root = new AdvancedDropdownItem($"{_title} {_pathTreeRoot.data.name}");

			for (int i = 0; i < _pathTreeRoot.children.Count; i++) {
				root.AddChild(CreateItem(_pathTreeRoot.children[i]));
			}

			return root;
		}

		private static Item CreateItem(TreeEntry<PathTree.Node<T>> treeEntry) {
			var item = new Item(treeEntry.data.data, treeEntry.data.name);

			var children = treeEntry.children;
			for (int i = 0; i < children.Count; i++) {
				item.AddChild(CreateItem(children[i]));
			}

			return item;
		}

		protected override void ItemSelected(AdvancedDropdownItem item) {
			base.ItemSelected(item);

			if (item is Item t) _onItemSelected.Invoke(t.data);
		}
	}

}
