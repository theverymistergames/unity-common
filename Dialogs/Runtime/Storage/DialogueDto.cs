using System;

namespace MisterGames.Dialogues.Storage {

    [Serializable]
    public sealed class DialogueFileDto {
        
        public Header header;
        public Role[] roles;
        public Branch[] branches;
        public ElementsArray[] localizations;
        
        [Serializable]
        public struct Header {
            public string id;
            public LocalizedContent[] localizations;
        }
        
        [Serializable]
        public struct Role {
            public string roleId;
            public LocalizedContent[] localizations;
        }

        [Serializable]
        public struct Branch {
            public string branchId;
            public LocalizedContent[] localizations;
        }

        [Serializable]
        public struct LocalizedContent {
            public string loc;
            public string content;
        }

        [Serializable]
        public struct ElementsArray {
            public string loc;
            public Element[] elements;    
        }
        
        [Serializable]
        public struct Element {
            public string elementId;
            public string branchId;
            public string roleId;
            public string[] lines;
        }
    }
    
}