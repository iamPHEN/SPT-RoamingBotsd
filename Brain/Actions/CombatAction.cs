using EFT;

namespace ExilBots.Layers
{
    public abstract class CombatAction : BotAction
    {
        public CombatAction(BotOwner botOwner, string name) : base(botOwner, name)
        {
        }
        public bool IsFightingEnemy()
        {
            return BotOwner.DogFight.DogFightState > BotDogFightStatus.none;
        }
    }
}
