namespace MisterGames.Blueprints.Core2 {

    public static class Ports {

        public static Port Enter(string name = "") {
            return new Port {
                name = name,
                isDataPort = false,
                isExitPort = false,
                hasDataType = false,
            };
        }

        public static Port Exit(string name = "") {
            return new Port {
                name = name,
                isDataPort = false,
                isExitPort = true,
                hasDataType = false,
            };
        }

        public static Port Input<T>(string name = "") {
            return new Port {
                name = name,
                isDataPort = true,
                isExitPort = false,
                hasDataType = true,
                DataType = typeof(T),
            };
        }

        public static Port Output<T>(string name = "") {
            return new Port {
                name = name,
                isDataPort = true,
                isExitPort = true,
                hasDataType = true,
                DataType = typeof(T),
            };
        }

        internal static Port Input(string name = "") {
            return new Port {
                name = name,
                isDataPort = true,
                isExitPort = false,
                hasDataType = false,
            };
        }

        internal static Port Output(string name = "") {
            return new Port {
                name = name,
                isDataPort = true,
                isExitPort = true,
                hasDataType = false,
            };
        }
    }

}
