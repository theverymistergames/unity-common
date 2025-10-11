using System;
using System.Collections.Generic;
using System.Text;
using MisterGames.Common.Lists;
using MisterGames.Common.Localization;
using MisterGames.Dialogues.Storage;
using Unity.Collections;
using UnityEngine;

namespace MisterGames.Dialogues.Editor.Parser {

    public static class DialogueDtoExtensions {

        private const string DefaultRoleId = "main";
        private const string DefaultBranchId = "main";
        
        public static bool ParseAndWrite(
            DialogueFileDto dto,
            Guid localizationTableGuid,
            ILocalizationTableStorage<string> writeLocalizationTable,
            IDialogueStorage writeDialogueStorage,
            IReadOnlyList<LocalizationSettings> localizationSettingsList) 
        {
            if (dto == null || writeDialogueStorage == null || writeLocalizationTable == null ||
                string.IsNullOrWhiteSpace(dto.header.id)) 
            {
                return false;
            }
            
            string dialogueId = dto.header.id.Trim();
            
            WriteHeader(dialogueId, dto, localizationTableGuid, writeLocalizationTable, writeDialogueStorage, localizationSettingsList);
            WriteRoles(dialogueId, dto, localizationTableGuid, writeLocalizationTable, writeDialogueStorage, localizationSettingsList);
            WriteBranches(dialogueId, dto, localizationTableGuid, writeLocalizationTable, writeDialogueStorage, localizationSettingsList);
            WriteElements(dialogueId, dto, localizationTableGuid, writeLocalizationTable, writeDialogueStorage, localizationSettingsList);
            
            return true;
        }

        private static void WriteHeader(
            string dialogueId,
            DialogueFileDto dto,
            Guid localizationTableGuid,
            ILocalizationTableStorage<string> writeLocalizationTable,
            IDialogueStorage writeDialogueStorage,
            IReadOnlyList<LocalizationSettings> localizationSettingsList) 
        {
            bool hasLocalizations = false;
            
            for (int i = 0; i < dto.header.localizations?.Length; i++) {
                var locData = dto.header.localizations[i];
                if (string.IsNullOrWhiteSpace(locData.loc)) continue;
                    
                writeLocalizationTable.SetValue(dialogueId, locData.content?.Trim(), LocaleExtensions.CreateLocale(locData.loc.Trim(), localizationSettingsList));
                hasLocalizations = true;
            }
            
            if (!hasLocalizations) {
                writeLocalizationTable.SetValue(dialogueId, dialogueId, LocaleExtensions.DefaultLocale);
            }
            
            writeDialogueStorage.SetDialogueId(LocalizationKeyExtensions.CreateLocalizationKey(dialogueId, localizationTableGuid));
        }

        private static void WriteRoles(
            string dialogueId,
            DialogueFileDto dto, 
            Guid localizationTableGuid,
            ILocalizationTableStorage<string> writeLocalizationTable,
            IDialogueStorage writeDialogueStorage,
            IReadOnlyList<LocalizationSettings> localizationSettingsList) 
        {
            string roleId;
            bool hasRoles = false;
            
            for (int i = 0; i < dto.roles?.Length; i++) {
                ref var roleData = ref dto.roles[i];
                if (string.IsNullOrEmpty(roleData.roleId)) continue;
                
                roleId = FormatRoleId(dialogueId, roleData.roleId.Trim(), i);
                writeDialogueStorage.AddRole(LocalizationKeyExtensions.CreateLocalizationKey(roleId, localizationTableGuid));
                bool hasLocalizations = false;
                
                for (int j = 0; j < roleData.localizations?.Length; j++) {
                    var locData = roleData.localizations[j];
                    if (string.IsNullOrEmpty(locData.loc)) continue;
                    
                    writeLocalizationTable.SetValue(roleId, locData.content?.Trim(), LocaleExtensions.CreateLocale(locData.loc.Trim(), localizationSettingsList));
                    hasLocalizations = true;
                }
                
                if (!hasLocalizations) {
                    writeLocalizationTable.SetValue(roleId, roleId, LocaleExtensions.DefaultLocale);
                }

                hasRoles = true;
            }
            
            if (hasRoles) return;
            
            roleId = FormatRoleId(dialogueId, DefaultRoleId, 0);

            writeLocalizationTable.SetValue(roleId, roleId, LocaleExtensions.DefaultLocale);
            writeDialogueStorage.AddRole(LocalizationKeyExtensions.CreateLocalizationKey(roleId, localizationTableGuid));
        }

        private static void WriteBranches(
            string dialogueId,
            DialogueFileDto dto, 
            Guid localizationTableGuid,
            ILocalizationTableStorage<string> writeLocalizationTable,
            IDialogueStorage writeDialogueStorage,
            IReadOnlyList<LocalizationSettings> localizationSettingsList) 
        {
            string branchId;
            bool hasBranches = false;
            
            for (int i = 0; i < dto.branches?.Length; i++) {
                ref var branchData = ref dto.branches[i];
                if (string.IsNullOrEmpty(branchData.branchId)) continue;

                branchId = FormatBranchId(dialogueId, branchData.branchId.Trim(), i);
                writeDialogueStorage.AddBranch(LocalizationKeyExtensions.CreateLocalizationKey(branchId, localizationTableGuid));
                bool hasLocalizations = false;
                    
                for (int j = 0; j < branchData.localizations?.Length; j++) {
                    var locData = branchData.localizations[j];
                    if (string.IsNullOrEmpty(locData.loc)) continue;
                    
                    writeLocalizationTable.SetValue(branchId, locData.content?.Trim(), LocaleExtensions.CreateLocale(locData.loc.Trim(), localizationSettingsList));
                    hasLocalizations = true;
                }

                if (!hasLocalizations) {
                    writeLocalizationTable.SetValue(branchId, null, LocaleExtensions.DefaultLocale);
                }
                
                hasBranches = true;
            }

            if (hasBranches) return;
            
            branchId = FormatBranchId(dialogueId, DefaultBranchId, 0);

            writeLocalizationTable.SetValue(branchId, branchId, LocaleExtensions.DefaultLocale);
            writeDialogueStorage.AddBranch(LocalizationKeyExtensions.CreateLocalizationKey(branchId, localizationTableGuid));
        }

        private static void WriteElements(
            string dialogueId,
            DialogueFileDto dto, 
            Guid localizationTableGuid,
            ILocalizationTableStorage<string> writeLocalizationTable,
            IDialogueStorage writeDialogueStorage,
            IReadOnlyList<LocalizationSettings> localizationSettingsList) 
        {
            var sb = new StringBuilder();
            var addedElementsHashes = new NativeHashSet<int>(100, Allocator.Temp);
            
            for (int i = 0; i < dto.localizations?.Length; i++) {
                ref var localizationData = ref dto.localizations[i];
                
                var locale = string.IsNullOrEmpty(localizationData.loc)
                    ? LocaleExtensions.DefaultLocale
                    : LocaleExtensions.CreateLocale(localizationData.loc, localizationSettingsList);

                string roleId = dto.roles?.Length > 0 ? dto.roles[0].roleId?.Trim() : null;
                string branchId = dto.branches?.Length > 0 ? dto.branches[0].branchId?.Trim() : null;

                roleId ??= DefaultRoleId;
                branchId ??= DefaultBranchId;
                
                int roleIndex = 0;
                int branchIndex = 0;
                
                for (int j = 0; j < localizationData.elements?.Length; j++) {
                    ref var element = ref localizationData.elements[j];

                    sb.Clear();

                    for (int k = 0; k < element.lines?.Length; k++) {
                        sb.Append(element.lines[k]);
                    }
                    
                    if (!string.IsNullOrEmpty(element.branchId)) {
                        branchId = element.branchId.Trim();
                        branchIndex = dto.branches.TryFindIndex(branchId, (branch, s) => branch.branchId?.Trim() == s);
                    }
                    
                    if (!string.IsNullOrEmpty(element.roleId)) {
                        roleId = element.roleId.Trim();
                        roleIndex = dto.roles.TryFindIndex(roleId, (role, s) => role.roleId?.Trim() == s);
                    }

                    string elementId = FormatElementId(dialogueId, branchId, roleId, element.elementId, j);
                    int elementHash = Animator.StringToHash(elementId);
                    
                    writeLocalizationTable.SetValue(elementId, sb.ToString(), locale);
                    
                    if (!addedElementsHashes.Add(elementHash)) continue;
                    
                    writeDialogueStorage.AddElement(new DialogueElement {
                        roleId = LocalizationKeyExtensions.CreateLocalizationKey(FormatRoleId(dialogueId, roleId, roleIndex), localizationTableGuid),
                        branchId = LocalizationKeyExtensions.CreateLocalizationKey(FormatBranchId(dialogueId, branchId, branchIndex), localizationTableGuid),
                        key = LocalizationKeyExtensions.CreateLocalizationKey(elementId, localizationTableGuid),
                    });
                }
            }

            addedElementsHashes.Dispose();
        }

        private static string FormatRoleId(string dialogueId, string roleId, int roleIndex) {
            return $"{dialogueId}_role" + 
                   (string.IsNullOrEmpty(roleId) ? $"-{roleIndex}" : $"_{roleId}");
        }
        
        private static string FormatBranchId(string dialogueId, string branchId, int branchIndex) {
            return $"{dialogueId}_branch" + 
                   (string.IsNullOrEmpty(branchId) ? $"-{branchIndex}" : $"_{branchId}");
        }

        private static string FormatElementId(string dialogueId, string branchId, string roleId, string elementId, int elementIndex) {
            return $"{dialogueId}_line{elementIndex:00}" +
                   (string.IsNullOrEmpty(branchId) ? "" : $"_{branchId}") +
                   (string.IsNullOrEmpty(roleId) ? "" : $"_{roleId}") + 
                   (string.IsNullOrWhiteSpace(elementId) ? "" : $"_{elementId}");
        }
    }
    
}