using ElPuebloDuermeDemo.Models;

namespace ElPuebloDuermeDemo.Models
{
    public class Player
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public Role Role { get; set; }
        public bool IsAlive { get; set; } = true;
        public bool HasUsedSpecialAbility { get; set; } = false;
        public int LastAbilityUsedRound { get; set; } = 0;
        public int VotesReceived { get; set; } = 0;
        public string? VotedFor { get; set; }
        public bool IsProtected { get; set; } = false;
        public string ConnectionId { get; set; } = string.Empty;

        // Para roles con habilidades que se recargan cada X turnos
        public bool CanUseAbility(int currentRound)
        {
            return Role switch
            {
                Role.Caballero => currentRound - LastAbilityUsedRound >= 2,
                Role.Medico => !HasUsedSpecialAbility,
                Role.Clerigo => !HasUsedSpecialAbility,
                Role.Detective => !HasUsedSpecialAbility, // Para la habilidad especial de matar
                _ => true
            };
        }

        public void UseAbility(int currentRound)
        {
            HasUsedSpecialAbility = true;
            LastAbilityUsedRound = currentRound;
        }

        public void ResetProtection()
        {
            IsProtected = false;
        }
    }
}