using System.Collections.Generic;
using System.Numerics;
using TiledSharp;

public class LevelLoader
{
    public static List<EnemyData> LoadLevel(string filePath)
    {
        var map = new TmxMap(filePath);
        var enemies = new List<EnemyData>();

        foreach (var objGroup in map.ObjectGroups)
        {
            if (objGroup.Name == "Enemies")
            {
                foreach (var obj in objGroup.Objects)
                {
                    Vector2 position = new Vector2((float)obj.X, (float)obj.Y);
                    int enemyType = int.Parse(obj.Type); // Предполагается, что тип врага указан в поле Type
                    enemies.Add(new EnemyData { Position = position, Type = enemyType });
                }
            }
        }

        return enemies;
    }
}

public class EnemyData
{
    public Vector2 Position { get; set; }
    public int Type { get; set; }
}