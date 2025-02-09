using System.Collections.Generic;
using System.Numerics;
using TiledSharp;

public class LevelData
{
    // List of enemies in the level
    public List<EnemyData> Enemies { get; set; }

    // The actual map data loaded from a Tiled TMX file
    public TmxMap Map { get; set; }
}

public class LevelLoader
{
    public static LevelData LoadLevel(string filePath)
    {
        // Load the TMX map from the given file path
        var map = new TmxMap(filePath);
        var enemies = new List<EnemyData>();

        // Iterate through all object groups in the map
        foreach (var objGroup in map.ObjectGroups)
        {
            // Look for the object group named "Enemies"
            if (objGroup.Name == "Enemies")
            {
                // Print all object group names for debugging
                Console.WriteLine($"Object groups in level: {string.Join(", ", map.ObjectGroups.Select(g => g.Name))}");

                // Iterate through objects in the "Enemies" group
                foreach (var obj in objGroup.Objects)
                {
                    // Extract enemy position from the object properties
                    Vector2 position = new Vector2((float)obj.X, (float)obj.Y);

                    // Convert enemy type from string to integer
                    int enemyType = int.Parse(obj.Type);

                    // Add enemy data to the list
                    enemies.Add(new EnemyData { Position = position, Type = enemyType });
                }
            }
        }

        // Print level details for debugging
        Console.WriteLine($"Level loaded: {filePath}, Enemies Count: {enemies.Count}");

        // Return the level data containing the enemies and map
        return new LevelData { Enemies = enemies, Map = map };
    }
}

public class EnemyData
{
    // Enemy position in the level
    public Vector2 Position { get; set; }

    // Type identifier of the enemy (used to determine its behavior or appearance)
    public int Type { get; set; }
}
