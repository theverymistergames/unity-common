using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MisterGames.Common.Editor.SubclassSelector {
	
	public class AdvancedTypePopupItem : AdvancedDropdownItem {
		
		public Type Type { get; }

		public AdvancedTypePopupItem(Type type, string name) : base(name) {
			Type = type;
		}
	}

	public class AdvancedTypePopup : AdvancedDropdown {
		
		public event Action<AdvancedTypePopupItem> OnItemSelected = delegate {  };
		
		private const int MaxNamespaceNestCount = 16;
		private static readonly float HeaderHeight = EditorGUIUtility.singleLineHeight * 2f;

		private readonly Type[] _types;
		
		public AdvancedTypePopup(IEnumerable<Type> types, int maxLineCount, AdvancedDropdownState state) : base(state) {
			_types = types.ToArray();
			minimumSize = new Vector2(minimumSize.x, EditorGUIUtility.singleLineHeight * maxLineCount + HeaderHeight);
		}

		protected override AdvancedDropdownItem BuildRoot() {
			var root = new AdvancedDropdownItem("Select Type");
			AddTo(root,_types);
			return root;
		}

		protected override void ItemSelected(AdvancedDropdownItem item) {
			base.ItemSelected(item);
			if (item is AdvancedTypePopupItem typePopupItem) {
				OnItemSelected.Invoke(typePopupItem);
			}
		}

		private static void AddTo(AdvancedDropdownItem root, IEnumerable<Type> types) {
			int itemCount = 0;

			var nullItem = new AdvancedTypePopupItem(null, SubclassSelectorUtils.NullDisplayName) { id = itemCount++ };
			root.AddChild(nullItem);

			var typeArray = SubclassSelectorUtils.OrderByType(types).ToArray();

			bool isSingleNamespace = true;
			string[] namespaces = new string[MaxNamespaceNestCount];
			
			foreach (var type in typeArray) {
				string[] splittedTypePath = SubclassSelectorUtils.GetSplittedTypePath(type);
				if (splittedTypePath.Length <= 1) {
					continue;
				}
				
				for (int k = 0; splittedTypePath.Length - 1 > k; k++) {
					string ns = namespaces[k];
					
					if (ns == null) {
						namespaces[k] = splittedTypePath[k];
						continue;
					}

					if (ns == splittedTypePath[k]) {
						continue;
					}
					
					isSingleNamespace = false;
					break;
				}
			}

			foreach (var type in typeArray) {
				string[] splittedTypePath = SubclassSelectorUtils.GetSplittedTypePath(type);
				if (splittedTypePath.Length == 0) {
					continue;
				}

				var parent = root;
				if (!isSingleNamespace) {
					for (int k = 0; splittedTypePath.Length - 1 > k; k++) {
						var foundItem = GetItem(parent,splittedTypePath[k]);
						if (foundItem != null) {
							parent = foundItem;
							continue;
						}
						
						var newItem = new AdvancedDropdownItem(splittedTypePath[k]) { id = itemCount++ };
						parent.AddChild(newItem);
						parent = newItem;
					}
				}

				string path = splittedTypePath[splittedTypePath.Length - 1];
				var item = new AdvancedTypePopupItem(type, ObjectNames.NicifyVariableName(path)) { id = itemCount++ };
				parent.AddChild(item);
			}
		}

		private static AdvancedDropdownItem GetItem(AdvancedDropdownItem parent, string name) {
			foreach (var item in parent.children) {
				if (item.name == name) {
					return item;
				}
			}
			
			return null;
		}
	}
}
