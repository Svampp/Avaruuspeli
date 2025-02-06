using System.Collections.Generic;
using System.Numerics;
using TiledSharp;

public class LevelData
{
    public List<EnemyData> Enemies { get; set; }
    public TmxMap Map { get; set; }  // Добавляем саму карту
}

public class LevelLoader
{
    public static LevelData LoadLevel(string filePath)
    {
        var map = new TmxMap(filePath);
        var enemies = new List<EnemyData>();

        foreach (var objGroup in map.ObjectGroups)
        {
            if (objGroup.Name == "Enemies")
            {
                Console.WriteLine($"Object groups in level: {string.Join(", ", map.ObjectGroups.Select(g => g.Name))}");


                foreach (var obj in objGroup.Objects)
                {
                    Vector2 position = new Vector2((float)obj.X, (float)obj.Y);
                    int enemyType = int.Parse(obj.Type);
                    enemies.Add(new EnemyData { Position = position, Type = enemyType });
                }
            }
        }

        Console.WriteLine($"Level loaded: {filePath}, Enemies Count: {enemies.Count}");
        return new LevelData { Enemies = enemies, Map = map };
    }
}

public class EnemyData
{
    public Vector2 Position { get; set; }
    public int Type { get; set; }
}