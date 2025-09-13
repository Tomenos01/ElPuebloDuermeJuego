using ElPuebloDuermeDemo.Models;

namespace ElPuebloDuermeDemo.Services
{
    public interface IRoleAssignmentService
    {
        List<Role> GenerateRoleDistribution(int playerCount);
        void AssignRoles(List<Player> players);
    }

    public class RoleAssignmentService : IRoleAssignmentService
    {
        private readonly Random _random = new();

        public List<Role> GenerateRoleDistribution(int playerCount)
        {
            var roles = new List<Role>();

            // Reglas de distribución de roles según el número de jugadores
            switch (playerCount)
            {
                case 4:
                    roles.AddRange([Role.Asesino, Role.Detective, Role.Aldeano, Role.Aldeano]);
                    break;
                case 5:
                    roles.AddRange([Role.Asesino, Role.Detective, Role.Medico, Role.Aldeano, Role.Aldeano]);
                    break;
                case 6:
                    roles.AddRange([Role.Asesino, Role.Detective, Role.Medico, Role.Caballero, Role.Aldeano, Role.Aldeano]);
                    break;
                case 7:
                    roles.AddRange([Role.Asesino, Role.Detective, Role.Medico, Role.Caballero, Role.Clerigo, Role.Aldeano, Role.Aldeano]);
                    break;
                case 8:
                    roles.AddRange([Role.Asesino, Role.Detective, Role.Medico, Role.Caballero, Role.Clerigo, Role.Bufon, Role.Aldeano, Role.Aldeano]);
                    break;
                case 9:
                    roles.AddRange([Role.Asesino, Role.Asesino, Role.Detective, Role.Medico, Role.Caballero, Role.Clerigo, Role.Bufon, Role.Aldeano, Role.Aldeano]);
                    break;
                case 10:
                    roles.AddRange([Role.Asesino, Role.Asesino, Role.Detective, Role.Medico, Role.Caballero, Role.Clerigo, Role.Bufon, Role.Aldeano, Role.Aldeano, Role.Aldeano]);
                    break;
                case 11:
                    roles.AddRange([Role.Asesino, Role.Asesino, Role.Detective, Role.Medico, Role.Caballero, Role.Clerigo, Role.Bufon, Role.Aldeano, Role.Aldeano, Role.Aldeano, Role.Aldeano]);
                    break;
                case 12:
                    roles.AddRange([Role.Asesino, Role.Asesino, Role.Aldeano, Role.Detective, Role.Medico, Role.Caballero, Role.Clerigo, Role.Bufon, Role.Aldeano, Role.Aldeano, Role.Aldeano, Role.Aldeano]);
                    break;
                default:
                    throw new ArgumentException($"No se soporta un juego con {playerCount} jugadores");
            }

            return roles;
        }

        public void AssignRoles(List<Player> players)
        {
            if (players == null || players.Count == 0)
                throw new ArgumentException("La lista de jugadores no puede estar vacía");

            var roles = GenerateRoleDistribution(players.Count);
            
            // Mezclar los roles aleatoriamente
            for (int i = roles.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (roles[i], roles[j]) = (roles[j], roles[i]);
            }

            // Asignar roles a jugadores
            for (int i = 0; i < players.Count; i++)
            {
                players[i].Role = roles[i];
                players[i].IsAlive = true;
                players[i].HasUsedSpecialAbility = false;
                players[i].LastAbilityUsedRound = 0;
                players[i].VotesReceived = 0;
                players[i].VotedFor = null;
                players[i].IsProtected = false;
            }
        }
    }
}