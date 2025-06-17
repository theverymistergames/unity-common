using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Trees;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MisterGames.Common.Editor.Views {

	public enum AdvancedDropdownSelectType {
		Item,
		ItemIcon,
	}
	
	public sealed class AdvancedDropdown<T> : AdvancedDropdown {

		private const int _iconPosXMax = 12;
		
		private readonly string _title;
		private readonly Action<T, AdvancedDropdownSelectType> _onItemSelected;
		private readonly Func<IEnumerable<TreeEntry<PathTree.Node<T>>>, IEnumerable<TreeEntry<PathTree.Node<T>>>> _sort;
		private readonly Func<T, Texture2D> _getIcon;
		private readonly Func<T, bool> _getEnabled;
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
			Action<T, AdvancedDropdownSelectType> onItemSelected,
			char separator = '/',
			Func<IEnumerable<TreeEntry<PathTree.Node<T>>>, IEnumerable<TreeEntry<PathTree.Node<T>>>> sort = null,
			Func<T, Texture2D> getIcon = null,
			Func<T, bool> getEnabled = null
		) : base(new AdvancedDropdownState()) {
			_title = title;
			_onItemSelected = onItemSelected;
			_getIcon = getIcon;
			_getEnabled = getEnabled;
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

		private Item CreateItem(TreeEntry<PathTree.Node<T>> treeEntry) {
			var item = new Item(treeEntry.data.data, treeEntry.data.name);

			var children = treeEntry.children;
			for (int i = 0; i < children.Count; i++) {
				item.AddChild(CreateItem(children[i]));
			}

			var icon = _getIcon?.Invoke(treeEntry.data.data);
			if (icon != null) item.icon = icon;
			
			item.enabled = _getEnabled?.Invoke(treeEntry.data.data) ?? true;
			
			return item;
		}

		protected override void ItemSelected(AdvancedDropdownItem item) {
			base.ItemSelected(item);

			var selectType = Event.current.mousePosition.x <= _iconPosXMax && item.icon != null 
				? AdvancedDropdownSelectType.ItemIcon 
				: AdvancedDropdownSelectType.Item;
			
			if (item is Item t) _onItemSelected.Invoke(t.data, selectType);
		}
	}

}
