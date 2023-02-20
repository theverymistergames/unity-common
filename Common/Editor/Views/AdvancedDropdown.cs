using System;
using System.Collections.Generic;
using MisterGames.Common.Editor.Tree;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MisterGames.Common.Editor {

	public sealed class AdvancedDropdown<T> : AdvancedDropdown {

		private const int DEFAULT_LINES_COUNT = 10;

		private readonly string _title;
		private readonly IEnumerable<T> _items;
		private readonly Func<T, string> _getItemPath;
		private readonly Action<T> _onItemSelected;
		private readonly char _separator;
		private readonly Func<IEnumerable<TreeEntry<PathTree.Node<T>>>, IEnumerable<TreeEntry<PathTree.Node<T>>>> _sort;

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
			int linesCount = DEFAULT_LINES_COUNT,
			Func<IEnumerable<TreeEntry<PathTree.Node<T>>>, IEnumerable<TreeEntry<PathTree.Node<T>>>> sort = null
		) : base(new AdvancedDropdownState()) {
			_title = title;
			_items = items;
			_getItemPath = getItemPath;
			_onItemSelected = onItemSelected;
			_separator = separator;
			_sort = sort;
			minimumSize = new Vector2(minimumSize.x, (2 + linesCount) * EditorGUIUtility.singleLineHeight);
		}

		protected override AdvancedDropdownItem BuildRoot() {
			var root = new AdvancedDropdownItem(_title);
			var pathTreeRoot = PathTree.CreateTree(_items, _getItemPath, _separator, _sort);

			for (int i = 0; i < pathTreeRoot.children.Count; i++) {
				root.AddChild(CreateItem(pathTreeRoot.children[i]));
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
