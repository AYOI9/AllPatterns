using System;
using System.Collections.Generic;

namespace RpgPatternSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            GameSettings settings1 = GameSettings.Instance;
            GameSettings settings2 = GameSettings.Instance;

            Console.WriteLine("RPG SYSTEM");
            Console.WriteLine($"Сложность: {settings1.Difficulty}");
            Console.WriteLine($"Максимальный уровень: {settings1.MaxLevel}");
            Console.WriteLine($"Настройки совпадают: {settings1 == settings2}\n");

            CharacterFactory warriorFactory = new WarriorFactory();
            ICharacter baseWarrior = warriorFactory.CreateCharacter();
            Console.WriteLine($"Создан персонаж: {baseWarrior.ClassName}\n");

            IEquipmentFactory equipFactory = new WarriorEquipmentFactory();
            IWeapon rawWeapon = equipFactory.CreateWeapon();
            IArmor armor = equipFactory.CreateArmor();
            Console.WriteLine($"Оружие: {rawWeapon.GetName()}");
            Console.WriteLine($"Броня: {armor.GetName()}\n");

            IWeapon modifiedWeapon = rawWeapon;
            modifiedWeapon = new FireDecorator(modifiedWeapon);
            modifiedWeapon = new PoisonDecorator(modifiedWeapon);
            modifiedWeapon = new CriticalDecorator(modifiedWeapon);
            Console.WriteLine($"Итоговое оружие: {modifiedWeapon.GetName()}");
            Console.WriteLine($"Итоговый урон: {modifiedWeapon.GetDamage()}\n");

            CharacterBuilder builder = new CharacterBuilder();
            Character andrey = builder
                .SetName("Андрей")
                .SetLevel(10)
                .SetHealth(150)
                .SetWeapon(modifiedWeapon)
                .SetArmor(armor)
                .SetAttack(new PhysicalAttack())
                .Build();

            Console.WriteLine($"Собран персонаж: {andrey.Name}, Класс: Warrior, Уровень: {andrey.Level}, HP: {andrey.Health}");
            andrey.Attack();

            andrey.AttackBehavior = new MagicAttack();
            andrey.Attack();
            Console.WriteLine();

            OldCombatSystem oldSystem = new OldCombatSystem();
            IAttack adaptedAttack = new OldCombatAdapter(oldSystem, 50);
            andrey.AttackBehavior = adaptedAttack;
            andrey.Attack();
            Console.WriteLine();

            Character tolik = builder
                .SetName("Толик")
                .SetLevel(10)
                .SetHealth(150)
                .SetWeapon(new Sword())
                .SetArmor(new HeavyArmor())
                .SetAttack(new PhysicalAttack())
                .Build();

            Character tolikClone = tolik.Clone();
            tolikClone.Name = "Толик Clone";
            tolikClone.Level = 20;

            Console.WriteLine($"Оригинал: {tolik.Name}, Уровень: {tolik.Level}");
            Console.WriteLine($"Копия:    {tolikClone.Name}, Уровень: {tolikClone.Level}\n");

            Squad army = new Squad();
            army.AddUnit(new Character { Name = "Гром", AttackBehavior = new PhysicalAttack() });
            army.AddUnit(new Character { Name = "Антондас", AttackBehavior = new MagicAttack() });

            Squad squad1 = new Squad();
            squad1.AddUnit(new Character { Name = "Снайпер 1", AttackBehavior = new RangedAttack() });
            squad1.AddUnit(new Character { Name = "Пехотинец 1", AttackBehavior = new PhysicalAttack() });
            army.AddUnit(squad1);

            Console.WriteLine("--- АТАКА ВСЕЙ АРМИИ ---");
            army.Attack();

            Console.ReadLine();
        }
    }

    public class GameSettings
    {
        private static GameSettings _instance;
        private static readonly object _lock = new object();

        public string GameName { get; private set; }
        public string Difficulty { get; private set; }
        public int MaxLevel { get; private set; }

        private GameSettings()
        {
            GameName = "RPG SYSTEM";
            Difficulty = "Normal";
            MaxLevel = 100;
        }

        public static GameSettings Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new GameSettings();
                    }
                    return _instance;
                }
            }
        }
    }

    public interface IWeapon
    {
        string GetName();
        int GetDamage();
    }

    public interface IArmor
    {
        string GetName();
        int GetDefense();
    }

    public class Sword : IWeapon
    {
        public string GetName() => "Sword";
        public int GetDamage() => 15;
    }

    public class Bow : IWeapon
    {
        public string GetName() => "Bow";
        public int GetDamage() => 12;
    }

    public class MagicStaff : IWeapon
    {
        public string GetName() => "Magic Staff";
        public int GetDamage() => 10;
    }

    public class HeavyArmor : IArmor
    {
        public string GetName() => "Heavy Armor";
        public int GetDefense() => 30;
    }

    public class LightArmor : IArmor
    {
        public string GetName() => "Light Armor";
        public int GetDefense() => 15;
    }

    public class Robe : IArmor
    {
        public string GetName() => "Robe";
        public int GetDefense() => 5;
    }

    public abstract class WeaponDecorator : IWeapon
    {
        protected IWeapon _decoratee;
        public WeaponDecorator(IWeapon weapon) => _decoratee = weapon;
        public virtual string GetName() => _decoratee.GetName();
        public virtual int GetDamage() => _decoratee.GetDamage();
    }

    public class FireDecorator : WeaponDecorator
    {
        public FireDecorator(IWeapon weapon) : base(weapon) { }
        public override string GetName() => "Fire " + base.GetName();
        public override int GetDamage() => base.GetDamage() + 5;
    }

    public class PoisonDecorator : WeaponDecorator
    {
        public PoisonDecorator(IWeapon weapon) : base(weapon) { }
        public override string GetName() => "Poison " + base.GetName();
        public override int GetDamage() => base.GetDamage() + 3;
    }

    public class CriticalDecorator : WeaponDecorator
    {
        public CriticalDecorator(IWeapon weapon) : base(weapon) { }
        public override string GetName() => "Critical " + base.GetName();
        public override int GetDamage() => base.GetDamage() * 2;
    }

    public interface IEquipmentFactory
    {
        IWeapon CreateWeapon();
        IArmor CreateArmor();
    }

    public class WarriorEquipmentFactory : IEquipmentFactory
    {
        public IWeapon CreateWeapon() => new Sword();
        public IArmor CreateArmor() => new HeavyArmor();
    }

    public class MageEquipmentFactory : IEquipmentFactory
    {
        public IWeapon CreateWeapon() => new MagicStaff();
        public IArmor CreateArmor() => new Robe();
    }

    public class ArcherEquipmentFactory : IEquipmentFactory
    {
        public IWeapon CreateWeapon() => new Bow();
        public IArmor CreateArmor() => new LightArmor();
    }

    public interface IUnit
    {
        void Attack();
    }

    public interface IAttack
    {
        void Attack(string characterName);
    }

    public class PhysicalAttack : IAttack
    {
        public void Attack(string characterName) => Console.WriteLine($"{characterName} использует физическую атаку.");
    }

    public class MagicAttack : IAttack
    {
        public void Attack(string characterName) => Console.WriteLine($"{characterName} использует магическую атаку.");
    }

    public class RangedAttack : IAttack
    {
        public void Attack(string characterName) => Console.WriteLine($"{characterName} стреляет из лука.");
    }

    public interface ICharacter
    {
        string ClassName { get; }
    }

    public class Character : IUnit, ICharacter
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int Health { get; set; }
        public IWeapon Weapon { get; set; }
        public IArmor Armor { get; set; }
        public IAttack AttackBehavior { get; set; }

        public virtual string ClassName => "Generic Character";

        public void Attack()
        {
            AttackBehavior?.Attack(Name);
        }

        public Character Clone()
        {
            return new Character
            {
                Name = this.Name,
                Level = this.Level,
                Health = this.Health,
                Weapon = this.Weapon,
                Armor = this.Armor,
                AttackBehavior = this.AttackBehavior
            };
        }
    }

    public class Warrior : Character { public override string ClassName => "Warrior"; }
    public class Mage : Character { public override string ClassName => "Mage"; }
    public class Archer : Character { public override string ClassName => "Archer"; }

    public abstract class CharacterFactory
    {
        public abstract ICharacter CreateCharacter();
    }

    public class WarriorFactory : CharacterFactory
    {
        public override ICharacter CreateCharacter() => new Warrior();
    }

    public class MageFactory : CharacterFactory
    {
        public override ICharacter CreateCharacter() => new Mage();
    }

    public class ArcherFactory : CharacterFactory
    {
        public override ICharacter CreateCharacter() => new Archer();
    }

    public class CharacterBuilder
    {
        private string _name;
        private int _level;
        private int _health;
        private IWeapon _weapon;
        private IArmor _armor;
        private IAttack _attack;

        public CharacterBuilder SetName(string name) { _name = name; return this; }
        public CharacterBuilder SetLevel(int level) { _level = level; return this; }
        public CharacterBuilder SetHealth(int health) { _health = health; return this; }

        public CharacterBuilder SetWeapon(IWeapon weapon) { _weapon = weapon; return this; }
        public CharacterBuilder SetArmor(IArmor armor) { _armor = armor; return this; }
        public CharacterBuilder SetAttack(IAttack attack) { _attack = attack; return this; }

        public Character Build()
        {
            return new Character
            {
                Name = _name,
                Level = _level,
                Health = _health,
                Weapon = _weapon,
                Armor = _armor,
                AttackBehavior = _attack
            };
        }
    }

    public class OldCombatSystem
    {
        public void MakeHit(string name, int power)
        {
            Console.WriteLine($"{name} наносит удар силой {power}");
        }
    }

    public class OldCombatAdapter : IAttack
    {
        private readonly OldCombatSystem _oldCombatSystem;
        private readonly int _defaultPower;

        public OldCombatAdapter(OldCombatSystem oldCombatSystem, int defaultPower)
        {
            _oldCombatSystem = oldCombatSystem;
            _defaultPower = defaultPower;
        }

        public void Attack(string characterName)
        {
            _oldCombatSystem.MakeHit(characterName, _defaultPower);
        }
    }

    public class Squad : IUnit
    {
        private readonly List<IUnit> _children = new List<IUnit>();

        public void AddUnit(IUnit unit)
        {
            _children.Add(unit);
        }

        public void RemoveUnit(IUnit unit)
        {
            _children.Remove(unit);
        }

        public void Attack()
        {
            foreach (var unit in _children)
            {
                unit.Attack();
            }
        }
    }
}
