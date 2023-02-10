using System;

namespace MisterGames.Input.Activation {

    public interface IKeyActivationStrategy {

        Action OnUse { set; }

        void OnPressed();
        void OnReleased();

        void Interrupt();

        void OnUpdate(float dt);
    }

}
