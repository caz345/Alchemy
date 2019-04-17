using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Alchemy
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var start = new QuickStart(string.Join(' ', args));
                new AlchemyModule(start);
            }
            while (true)
            { 
                new AlchemyModule();
            }
        }
    }

    public enum AlchemyTrigger
    {
        CommandWord = 1,
        Contact = 2,
        Timed = 3
    }

    public class AlchemyModule
    {
        private List<AlchemySpell> Spells = new List<AlchemySpell>();
        private int PhysicalDrain = 0;
        private int StunDrain = 0;

        public string SpellName { get; set; }
        public int Force { get; set; }
        public int Reagents { get; set; }
        public AlchemyTrigger Trigger { get; set; }
        public int SpellDrainBonus { get; set; }
        public int Alchemy { get; set; }
        public int MagicRating { get; set; }
        public int ResistanceCheck { get; set; }

        private QuickStart Saved;

        public AlchemyModule()
        {
            Init();
            Run();
            MainLoop();
        }

        public AlchemyModule(QuickStart quickstart)
        {
            Saved = new QuickStart()
            {
                Alchemy = quickstart.Alchemy,
                MagicRating = quickstart.MagicRating,
                ResistanceCheck = quickstart.ResistanceCheck
            };
            Alchemy = quickstart.Alchemy;
            MagicRating = quickstart.MagicRating;
            ResistanceCheck = quickstart.ResistanceCheck;
            foreach(var spell in quickstart.Spells)
            {
                SpellName = spell.SpellName;
                Force = spell.Force;
                Reagents = 0;
                Trigger = spell.Trigger;
                SpellDrainBonus = spell.SpellDrainBonus;
                Run();
            }
            MainLoop();
        }

        private void MainLoop()
        {
            while (true)
            {
                Console.WriteLine("Enter option");
                Console.WriteLine("1. Repeat");
                Console.WriteLine("2. New Spell");
                Console.WriteLine("3. Finish and get summary");
                var option = ReadInt();
                Console.Clear();
                if (option == 2)
                {
                    NewSpellInit();
                }
                else if(option == 3)
                {
                    break;
                }
                Run();
            }

            Console.WriteLine($"Took {Spells.Sum(s => s.Force)} minutes to complete");
            Console.WriteLine("========");
            Console.WriteLine("=Spells=");
            Console.WriteLine("========");
            foreach (var spell in Spells)
                Console.WriteLine(spell.ToString());
            Console.WriteLine("=======");
            Console.WriteLine("=Drain=");
            Console.WriteLine("=======");
            Console.WriteLine($"Physical: {PhysicalDrain}");
            Console.WriteLine($"Stun: {StunDrain}");

            Console.WriteLine("Generate quickstart string? y/n");
            var input = Console.ReadLine();
            if (input == "y")
            {
                Console.WriteLine("Pass as an argument to the app");
                Console.WriteLine(Saved.ToString());
                Console.ReadKey();
            }
            else if (input != "n")
            {
                Console.WriteLine("That wasn't an n, asshole");
                Thread.Sleep(400);
            }
            Console.Clear();
        }

        private void Init()
        {
            Console.WriteLine("What's the spell name?");
            SpellName = Console.ReadLine();
            Console.WriteLine("What's the spell force?");
            Force = ReadInt();
            Console.WriteLine("How many reagents?");
            Reagents = ReadInt();
            Trigger = ReadTrigger();
            Console.WriteLine("Enter x for the spell drain: f - x");
            SpellDrainBonus = ReadInt();
            Console.WriteLine("Enter Alchemy skill rating");
            Alchemy = ReadInt();
            Console.WriteLine("Enter Magic rating");
            MagicRating = ReadInt();
            Console.WriteLine("Enter drain resistance test");
            ResistanceCheck = ReadInt();
            Console.Clear();

            Saved = new QuickStart()
            {
                Alchemy = Alchemy,
                MagicRating = MagicRating,
                ResistanceCheck = ResistanceCheck
            };
        }

        private void NewSpellInit()
        {
            Console.WriteLine("What's the spell name?");
            SpellName = Console.ReadLine();
            Console.WriteLine("What the spell force?");
            Force = ReadInt();
            Console.WriteLine("How many reagents?");
            Reagents = ReadInt();
            Trigger = ReadTrigger();
            Console.WriteLine("Enter x for the spell drain: f - x");
            SpellDrainBonus = ReadInt();
            Console.Clear();
            Saved.Spells.Add(new SavedSpell
            {
                Force = Force,
                SpellDrainBonus = SpellDrainBonus,
                SpellName = SpellName,
                Trigger = Trigger
            });
        }

        private int ReadInt()
        {
            while (true)
            {
                var input = Console.ReadLine();
                int inputInt;
                if (int.TryParse(input, out inputInt))
                {
                    return inputInt;
                }
                Console.WriteLine("That wasn't a number, idiot");
            }
        }

        private AlchemyTrigger ReadTrigger()
        {
            Console.WriteLine("What's the trigger type");
            Console.WriteLine("Enter number");
            Console.WriteLine("1. Command Word");
            Console.WriteLine("2. Contact");
            Console.WriteLine("3. Timed");
            var number = ReadInt();
            return (AlchemyTrigger)(number);
        }

        private void Run()
        {
            Saved.Spells.Add(new SavedSpell
            {
                Force = Force,
                SpellDrainBonus = SpellDrainBonus,
                SpellName = SpellName,
                Trigger = Trigger
            });
            var forceLimit = Math.Max(Force, Reagents);
            var skillcheck = DiceRoll(Alchemy + MagicRating, forceLimit);
            var alchemyDefense = DiceRoll(Force);
            var potency = skillcheck - alchemyDefense;
            var fullPotency = potency * 2;
            var lasts = fullPotency + potency;
            var triggerDrain = 0;
            var drainPhysical = skillcheck > MagicRating;
            var drainDamageString = drainPhysical ? "physical" : "stun";
            switch (Trigger)
            {
                case AlchemyTrigger.CommandWord:
                    triggerDrain = 2;
                    break;
                case AlchemyTrigger.Contact:
                    triggerDrain = 1;
                    break;
                case AlchemyTrigger.Timed:
                    triggerDrain = 2;
                    break;
            }
            var drain = Math.Max(Force - SpellDrainBonus, 0) + triggerDrain;
            var drainResistance = DiceRoll(ResistanceCheck);
            var drainDamage = Math.Max(0, drain - drainResistance);
            if (potency < 1)
            {
                Console.WriteLine($"Spell Failed {skillcheck} v. {alchemyDefense}");
            }
            else
            {
                Console.WriteLine($"Potency: {potency} | {skillcheck} v. {alchemyDefense}");
                Console.WriteLine($"Force: {Force}");
                Console.WriteLine($"Lasts at full potency for: {fullPotency} hours");
                Console.WriteLine($"Total lifespan: {lasts} hours");
                Spells.Add(new AlchemySpell
                {
                    Force = Force,
                    LastsFull = fullPotency,
                    LastsTotal = lasts,
                    Potency = potency,
                    SpellName = SpellName,
                    Trigger = Trigger
                });
            }
            Console.Write($"Drain: {drainDamage} {drainDamageString} damage | {drain} v. {drainResistance}");
            if (drainPhysical) PhysicalDrain += drainDamage;
            else StunDrain += drainDamage;
            Console.WriteLine();
            Console.WriteLine();

        }

        private int DiceRoll(int dice, int limit = int.MaxValue)
        {
            var rand = new Random();
            var hits = 0;
            for (int i = 0; i < dice; i++)
            {
                if (rand.Next(1, 7) >= 5) hits++;
            }
            return Math.Min(hits, limit);
        }
    }

    public class AlchemySpell
    {
        public string SpellName { get; set; }
        public int Force { get; set; }
        public int Potency { get; set; }
        public int LastsFull { get; set; }
        public int LastsTotal { get; set; }
        public AlchemyTrigger Trigger { get; set; }

        public override string ToString()
        {
            return $"{SpellName} | {Trigger} | Dice Pool: {Force + Potency} | Full Potency Time: {LastsFull} hours | Lasts: {LastsTotal} hours";
        }
    }

    public class QuickStart
    {
        public int Alchemy { get; set; }
        public int MagicRating { get; set; }
        public int ResistanceCheck { get; set; }
        public List<SavedSpell> Spells = new List<SavedSpell>();

        public QuickStart(string line)
        {
            var splitString = line.Split("||");
            parse(splitString[0]);
            for(int i = 1; i < splitString.Length; i++)
            {
                Spells.Add(new SavedSpell(splitString[i]));
            }
        }

        public QuickStart() { }

        public override string ToString()
        {
            var stringList = new List<string> { HeaderToString() };
            stringList.AddRange(Spells.Select(s => s.ToString()));
            return string.Join("||", stringList);
        }

        private void parse(string line)
        {
            var splitString = line.Split('|');
            Alchemy = int.Parse(splitString[0]);
            MagicRating = int.Parse(splitString[1]);
            ResistanceCheck = int.Parse(splitString[2]);
        }

        private string HeaderToString()
        {
            return $"{Alchemy}|{MagicRating}|{ResistanceCheck}";
        }
    }

    public class SavedSpell
    {
        public string SpellName { get; set; }
        public int Force { get; set; }
        public AlchemyTrigger Trigger { get; set; }
        public int SpellDrainBonus { get; set; }

        public SavedSpell() { }

        public SavedSpell(string line)
        {
            var splitString = line.Split('|');
            SpellName = splitString[0];
            Trigger = Enum.Parse<AlchemyTrigger>(splitString[1]);
            Force = int.Parse(splitString[2]);
            SpellDrainBonus = int.Parse(splitString[3]);
        }

        public override string ToString()
        {
            return $"{SpellName}|{Trigger}|{Force}|{SpellDrainBonus}";
        }
    }

}
