using System;

namespace TkokDiscordBot.EntGaming.Dto
{
    public class LobbyStatus : IEquatable<LobbyStatus>
    {
        public LobbyStatus()
        {
        }

        public LobbyStatus(string gameName)
        {
            GameName = gameName;
        }

        public LobbyStatus(string gameName, int id, int players, int maxPlayers)
        {
            GameName = gameName;
            Id = id;
            Players = players;
            MaxPlayers = maxPlayers;
        }

        public string GameName { get; set; }
        public int? Id { get; set; }
        public int? Players { get; set; }
        public int? MaxPlayers { get; set; }

        public bool Equals(LobbyStatus other)
        {
            return !ReferenceEquals(other, null) &&
                   Id == other.Id &&
                   GameName == other.GameName &&
                   Players == other.Players &&
                   MaxPlayers == other.MaxPlayers;
        }

        public static bool operator ==(LobbyStatus status1, LobbyStatus status2)
        {
            if (ReferenceEquals(status1, null))
                return ReferenceEquals(status2, null);

            return status1.Equals(status2);
        }

        public static bool operator !=(LobbyStatus status1, LobbyStatus status2)
        {
            return !(status1 == status2);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LobbyStatus);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = GameName != null ? GameName.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ Id.GetHashCode();
                hashCode = (hashCode * 397) ^ Players.GetHashCode();
                hashCode = (hashCode * 397) ^ MaxPlayers.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            if (!MaxPlayers.HasValue || !Players.HasValue)
                return GameName;
            return $"{GameName} [{Players}/{MaxPlayers}]";
        }
    }
}