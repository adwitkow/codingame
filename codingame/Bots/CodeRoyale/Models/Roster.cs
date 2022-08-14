﻿public class Roster
{
    public Roster(Owner owner)
    {
        this.Owner = owner;
    }

    public Owner Owner { get; }

    public Unit Queen { get; set; }

    public List<Unit> Knights { get; set; } = new List<Unit>();

    public List<Unit> Archers { get; set; } = new List<Unit>();

    internal void Clear()
    {
        Knights.Clear();
        Archers.Clear();
    }
}