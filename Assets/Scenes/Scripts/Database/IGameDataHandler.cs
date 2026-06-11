using System.Threading.Tasks;

public interface IGameDataHandler
{
    Task SaveMissionResult(string playerName, int firesExtinguished, float duration);
    Task<int> GetHighScore(string playerName);
}