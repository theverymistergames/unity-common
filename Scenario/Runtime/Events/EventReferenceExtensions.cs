namespace MisterGames.Scenario.Events {

    public static class EventReferenceExtensions {

        public static int GetRaiseCount(this EventReference e) {
            return EventSystems.Global?.RaisedEvents?.TryGetValue(e, out int count) ?? false ? count : 0;
        }
        
        public static void Raise(this EventReference e, int add = 1) {
            EventSystems.Global?.Raise(e, add);
        }
        
        public static void SetCount(this EventReference e, int count) {
            EventSystems.Global?.SetCount(e, count);
        }
        
        public static void Subscribe(this EventReference e, IEventListener listener) {
            EventSystems.Global?.Subscribe(e, listener);
        }

        public static void Unsubscribe(this EventReference e, IEventListener listener) {
            EventSystems.Global?.Unsubscribe(e, listener);
        }
    }

}
