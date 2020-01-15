using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace VirtuPet
{
    class Program
    {
        static string dataFile = "data.txt";
        static Sequence sequence;
        static void Main(string[] args)
        {
            Start();
        }

        public static void Start()
        {
            Console.Clear();
            sequence = new Sequence(dataFile);
            sequence.PetInputSequence();
        }
    }

    class Sequence
    {
        private Pet pet;
        private string dataFile;

        public Sequence(string dataFile)
        {
            this.dataFile = dataFile;
            if(File.Exists(dataFile))
            {
                if (File.ReadAllText(dataFile) != "")
                {
                    string[] file = File.ReadAllLines(dataFile);
                    pet = new Pet(file[0], DateTime.Parse(file[8]), float.Parse(file[2]),
                        float.Parse(file[3]), float.Parse(file[4]), int.Parse(file[5]), int.Parse(file[6]), int.Parse(file[7]), DateTime.Parse(file[1]), dataFile);
                    return;
                }
            }
            pet = CreatePetSequence();
        }

        public void PetInputSequence()
        {
            if (pet.Dead())
            {
                pet.Kill(dataFile);
                Program.Start();
            }
            Console.Clear();
            Console.WriteLine("[0] Show status" + "\n" +
                "[1] Feed pet" + "\n" +
                "[2] Play with pet" + "\n" +
                "[3] Give pet nap" + "\n" +
                "[4] Change name" + "\n" +
                "[5] Kill pet" + "\n" +
                "[6] Quit application");
            string input = Console.ReadLine();
            switch (input)
            {
                case "0":
                    CreatePetStats();
                    Console.WriteLine("Press ENTER to continue.");
                    Console.ReadLine();
                    PetInputSequence();
                    break;
                case "1":
                    pet.Feed();
                    ReturnMenu();
                    break;
                case "2":
                    pet.Play();
                    ReturnMenu();
                    break;
                case "3":
                    pet.Nap();
                    ReturnMenu();
                    break;
                case "4":
                    ChangeNameSequence();
                    ReturnMenu();
                    break;
                case "5":
                    Console.Clear();
                    pet.Kill(dataFile);
                    ReturnMenu();
                    break;
                case "6":
                    File.Delete(dataFile);
                    File.AppendAllText(dataFile, pet.name + "\n"+ DateTime.Now + "\n" + pet.hunger + "\n" + pet.exhaustion + "\n" + pet.happiness + "\n" + pet.hungerTime 
                        + "\n" + pet.exhaustTime + "\n" + pet.boreTime + "\n" + pet.birth);
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Input \"" + input + "\" is not valid.");
                    PetInputSequence();
                    break;
            }  
        }

        public void CreatePetStats()
        {
            Console.Clear();
            string content =
                "Name: " + pet.name + "\n" +
                "Age: " + pet.age + " minutes" + "\n" +
                "Hunger: " + pet.hunger + "/100" + "\n" +
                "Tiredness: " + pet.exhaustion + "/100" + "\n" +
                "Happiness: " + pet.happiness + "/100";
            Console.WriteLine(content);
        }

        public Pet CreatePetSequence()
        {
            Console.Clear();
            Console.WriteLine("Enter the name of your new pet:");
            string name = Console.ReadLine();
            if (name != "")
            {
                Console.WriteLine("You have chosen the name \"" + name + "\".");
                Thread.Sleep(3000);
                return new Pet(name);
            }
            else return CreatePetSequence();
        }

        public void ChangeNameSequence()
        {
            Console.Clear();
            Console.WriteLine("Write new name: ");
            string name = Console.ReadLine();
            pet.name = name;
            Console.Clear();
            Console.WriteLine("You have chosen the name: \"" + pet.name + "\".");
        }
        public  void ReturnMenu()
        {
            pet.DisposeTimers();
            Console.WriteLine("Press ENTER to continue.");
            Console.ReadLine();
            pet.InitTimers();
            PetInputSequence();
        }
    }

    class Pet
    {
        public Pet(string name)
        {
            birth = DateTime.Now;
            this.name = name;
            hunger = 0;
            age = 0;
            exhaustion = 0;
            happiness = 50;
            Random r = new Random();
            hungerTime = r.Next(5000, 15000);
            exhaustTime = r.Next(5000, 15000);
            boreTime = r.Next(5000, 15000);
            InitTimers();
        }

        public Pet(string name, DateTime birth, float hunger, float exhaustion, float happiness, int hungerTime, int exhaustTime, int boreTime, DateTime appClose, string dataFile)
        {
            TimeSpan elapsedTime = DateTime.Now - appClose;
            age = (int)DateTime.Now.Subtract(birth).TotalMinutes;
            this.hunger = (int)((float)elapsedTime.TotalMilliseconds / hungerTime + hunger);
            this.exhaustion = (int)((float)elapsedTime.TotalMilliseconds / exhaustTime + exhaustion);
            this.happiness = (int)((float)elapsedTime.TotalMilliseconds / boreTime + happiness);
            this.name = name;
            this.birth = birth;
            this.hungerTime = hungerTime;
            this.exhaustTime = exhaustTime;
            this.boreTime = boreTime;
            InitTimers();
        }
        public float exhaustion;
        public int age;
        public string name;
        public float hunger;
        public float happiness;

        public int hungerTime;
        public int exhaustTime;
        public int boreTime;

        Timer hungerTimer;
        Timer exhaustTimer;
        Timer boreTimer;

        private bool napping = false;
        public DateTime birth;

        private void Hunger(object o)
        {
            if(hunger < 100)
            {
                hunger += 1;
            }
        }

        private void Exhaust(object o)
        {
            if(exhaustion < 100)
            {
                exhaustion += 1;
            }
        }

        private void Bore(object o)
        {
            if(happiness > 0)
            {
                happiness -= 1;
                return;
            }
        }

        public void Kill(string dataFile)
        {
            File.Delete(dataFile);
            Console.WriteLine("\"" + name + "\" is dead.");
            DisposeTimers();
            Thread.Sleep(5000);
            Program.Start();
        }

        public void KillBoot(string dataFile)
        {
            File.Delete(dataFile);
            Console.WriteLine("\"" + name + "\" is dead.");
            Thread.Sleep(5000);
            Program.Start();
        }

        public void Feed()
        {
            Console.Clear();
            hunger = hunger < 10 ? 0 : hunger - 10;
            Console.WriteLine("You gave your pet a snack. \"" + name + "\" now has " + hunger + "/100 hunger");
        }

        public void Play()
        {
            Console.Clear();
            if (!napping) happiness = happiness > 90 ? 100 : happiness + 10;
            Console.WriteLine("You played with your pet. \"" + name + "\" now has " + happiness + "/100 happiness");
        }

        public void Nap()
        {
            napping = true;
            exhaustTimer.Dispose();
            boreTimer.Dispose();
            while(napping)
            {
                Console.Clear();
                Console.WriteLine("Tiredness: " + exhaustion);
                Console.Write(name + " is napping");
                Thread.Sleep(1000);
                Console.Write(".");
                Thread.Sleep(1000);
                Console.Write(".");
                Thread.Sleep(1000);
                Console.Write(".");
                Thread.Sleep(1000);
                if(exhaustion == 0)
                {
                    napping = false;
                }
                exhaustion = exhaustion < 10 ? 0 : exhaustion - 10;
            }
            boreTimer = new Timer(Bore, null, 0, boreTime);
            exhaustTimer = new Timer(Exhaust, null, 0, exhaustTime);
            Console.Clear();
            Console.WriteLine("Your pet is fully rested.");
        }

        public void InitTimers()
        {
            hungerTimer = new Timer(Hunger, null, hungerTime, hungerTime);
            exhaustTimer = new Timer(Exhaust, null, exhaustTime, exhaustTime);
            boreTimer = new Timer(Bore, null, boreTime, boreTime);
        }

        public void DisposeTimers()
        {
            hungerTimer.Dispose();
            exhaustTimer.Dispose();
            boreTimer.Dispose();
        }

        public bool Dead()
        {
            if (exhaustion >= 100)
            {
                Console.Clear();
                Console.WriteLine("\"" + name + "\" is too exhausted. He/she collapses, never to wake up again.");
                return true;
            }
            if (happiness <= 0)
            {
                Console.Clear();
                Console.WriteLine("\"" + name + "\" was so bored, he/she (literally) died of boredom.");
                return true;
            }
            if (hunger >= 100)
            {
                Console.Clear();
                Console.WriteLine("Not being fed causes \"" + name + "\" to die of hunger.");
                return true;
            }
            return false;
        }
    }
}
