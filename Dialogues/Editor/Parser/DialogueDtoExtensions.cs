using System;
using System.Text;
using MisterGames.Common.Lists;
using MisterGames.Common.Localization;
using MisterGames.Dialogues.Storage;

namespace MisterGames.Dialogues.Editor.Parser {

    public static class DialogueDtoExtensions {

        private const string DefaultRoleId = "main";
        private const string DefaultBranchId = "main";
        
        public static bool ParseAndWrite(
            DialogueFileDto dto,
            Guid localizationTableGuid,
            ILocalizationTableStorage<string> writeLocalizationTable,
            IDialogueTableStorage writeDialogueTableStorage) 
        {
            if (dto == null || writeDialogueTableStorage == null || writeLocalizationTable == null ||
                string.IsNullOrWhiteSpace(dto.id)) 
            {
                return false;
            }
            
            string dialogueId = dto.id.Trim();
            
            WriteHeader(dialogueId, dto, localizationTableGuid, writeLocalizationTable, writeDialogueTableStorage);
            WriteRoles(dialogueId, dto, localizationTableGuid, writeLocalizationTable, writeDialogueTableStorage);
            WriteBranches(dialogueId, dto, localizationTableGuid, writeLocalizationTable, writeDialogueTableStorage);
            WriteElements(dialogueId, dto, localizationTableGuid, writeLocalizationTable, writeDialogueTableStorage);
            
            return true;
        }

        private static void WriteHeader(
            string dialogueId,
            DialogueFileDto dto,
            Guid localizationTableGuid,
            ILocalizationTableStorage<string> writeLocalizationTable,
            IDialogueTableStorage writeDialogueTableStorage) 
        {
            bool hasLocalizations = false;
            
            for (int i = 0; i < dto.titleLocalizations?.Length; i++) {
                var locData = dto.titleLocalizations[i];
                if (string.IsNullOrWhiteSpace(locData.loc)) continue;
                    
                writeLocalizationTable.SetValue(dialogueId, locData.content?.Trim(), LocaleExtensions.CreateLocale(locData.loc.Trim()));
                hasLocalizations = true;
            }
            
            if (!hasLocalizations) {
                writeLocalizationTable.SetValue(dialogueId, dialogueId, LocaleExtensions.DefaultLocale);
            }
            
            writeDialogueTableStorage.SetDialogueId(LocalizationKeyExtensions.CreateLocalizationKey(dialogueId, localizationTableGuid));
        }

        private static void WriteRoles(
            string dialogueId,
            DialogueFileDto dto, 
            Guid localizationTableGuid,
            ILocalizationTableStorage<string> writeLocalizationTable,
            IDialogueTableStorage writeDialogueTableStorage) 
        {
            string roleId;
            bool hasRoles = false;
            
            for (int i = 0; i < dto.roles?.Length; i++) {
                ref var roleData = ref dto.roles[i];
                if (string.IsNullOrEmpty(roleData.roleId)) continue;
                
                roleId = FormatRoleId(dialogueId, roleData.roleId.Trim(), i);
                writeDialogueTableStorage.AddRole(LocalizationKeyExtensions.CreateLocalizationKey(roleId, localizationTableGuid));
                bool hasLocalizations = false;
                
                for (int j = 0; j < roleData.localizations?.Length; j++) {
                    var locData = roleData.localizations[j];
                    if (string.IsNullOrEmpty(locData.loc)) continue;
                    
                    writeLocalizationTable.SetValue(roleId, locData.content?.Trim(), LocaleExtensions.CreateLocale(locData.loc.Trim()));
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
            writeDialogueTableStorage.AddRole(LocalizationKeyExtensions.CreateLocalizationKey(roleId, localizationTableGuid));
        }

        private static void WriteBranches(
            string dialogueId,
            DialogueFileDto dto, 
            Guid localizationTableGuid,
            ILocalizationTableStorage<string> writeLocalizationTable,
            IDialogueTableStorage writeDialogueTableStorage) 
        {
            string branchId;
            bool hasBranches = false;
            
            for (int i = 0; i < dto.branches?.Length; i++) {
                ref var branchData = ref dto.branches[i];
                if (string.IsNullOrEmpty(branchData.branchId)) continue;

                branchId = FormatBranchId(dialogueId, branchData.branchId.Trim(), i);
                writeDialogueTableStorage.AddBranch(LocalizationKeyExtensions.CreateLocalizationKey(branchId, localizationTableGuid));
                bool hasLocalizations = false;
                    
                for (int j = 0; j < branchData.localizations?.Length; j++) {
                    var locData = branchData.localizations[j];
                    if (string.IsNullOrEmpty(locData.loc)) continue;
                    
                    writeLocalizationTable.SetValue(branchId, locData.content?.Trim(), LocaleExtensions.CreateLocale(locData.loc.Trim()));
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
            writeDialogueTableStorage.AddBranch(LocalizationKeyExtensions.CreateLocalizationKey(branchId, localizationTableGuid));
        }

        private static void WriteElements(
            string dialogueId,
            DialogueFileDto dto, 
            Guid localizationTableGuid,
            ILocalizationTableStorage<string> writeLocalizationTable,
            IDialogueTableStorage writeDialogueTableStorage) 
        {
            var sb = new StringBuilder();

            string roleId = dto.roles?.Length > 0 ? dto.roles[0].roleId?.Trim() : null;
            string branchId = dto.branches?.Length > 0 ? dto.branches[0].branchId?.Trim() : null;

            roleId ??= DefaultRoleId;
            branchId ??= DefaultBranchId;

            int roleIndex = 0;
            int branchIndex = 0;

            for (int i = 0; i < dto.elements?.Length; i++) {
                ref var element = ref dto.elements[i];

                if (!string.IsNullOrEmpty(element.branchId)) {
                    branchId = element.branchId.Trim();
                    branchIndex = dto.branches?.TryFindIndex(branchId, (branch, s) => branch.branchId?.Trim() == s) ?? 0;
                }

                if (!string.IsNullOrEmpty(element.roleId)) {
                    roleId = element.roleId.Trim();
                    roleIndex = dto.roles?.TryFindIndex(roleId, (role, s) => role.roleId?.Trim() == s) ?? 0;
                }

                string elementId = FormatElementId(dialogueId, branchId, roleId, element.elementId, i);
                bool hasLocalizations = false;

                for (int j = 0; j < element.content?.Length; j++) {
                    ref var localizedLines = ref element.content[j];

                    var locale = string.IsNullOrWhiteSpace(localizedLines.loc)
                        ? LocaleExtensions.DefaultLocale
                        : LocaleExtensions.CreateLocale(localizedLines.loc.Trim());

                    sb.Clear();

                    int lines = localizedLines.lines?.Length ?? 0;
                    for (int k = 0; k < lines; k++) {
                        if (k < lines - 1) sb.AppendLine(localizedLines.lines![k]);
                        else sb.Append(localizedLines.lines![k]);
                    }

                    writeLocalizationTable.SetValue(elementId, sb.ToString(), locale);
                    hasLocalizations = true;
                }

                if (!hasLocalizations) {
                    writeLocalizationTable.SetValue(elementId, null, LocaleExtensions.DefaultLocale);
                }

                writeDialogueTableStorage.AddElement(new DialogueElement {
                    roleId = LocalizationKeyExtensions.CreateLocalizationKey(FormatRoleId(dialogueId, roleId, roleIndex), localizationTableGuid),
                    branchId = LocalizationKeyExtensions.CreateLocalizationKey(FormatBranchId(dialogueId, branchId, branchIndex), localizationTableGuid),
                    key = LocalizationKeyExtensions.CreateLocalizationKey(elementId, localizationTableGuid),
                });
            }
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