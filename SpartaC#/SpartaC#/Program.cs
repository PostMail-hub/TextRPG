using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Net.Mail;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography.X509Certificates;
using static Player;
using static Player.Inventory;

public class GameStart
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        ShopItemManager.InitializeShopItems();
        Menu.NameMeun();
    }
}
public static class GameData
{
    public static Player player = new Player();
}

public class Player
{
    public string? playerName;
    public JopType playerJob;
    public int Level = 1;
    public int Gold = 2000;

    public enum StatType { Atk, Def, Hp, }
    int[] playerStats = new int[3];
    public Player()
    {
        playerStats[(int)StatType.Atk] = 10;
        playerStats[(int)StatType.Def] = 5;
        playerStats[(int)StatType.Hp] = 100;
    }
    public int GetStat(StatType stat)
    {
        return playerStats[(int)stat];
    }
    public int GetBaseStat(StatType statType)
    {
        return playerStats[(int)statType];
    }

    public int GetTotalAttack() // 최종 공격력 계산하기
    {
        int baseAtk = playerStats[(int)StatType.Atk];
        int equipmentAtk = 0;

        foreach (var item in Inventory.items)
        {
            if (item.IsEquipped)
            {
                var itemData = ShopItemManager.items.Find(x => x.IName == item.Name);
                if (itemData != null)
                {
                    equipmentAtk += itemData.IAtk;
                }
            }
        }
        return baseAtk + equipmentAtk;
    }
    public int GetTotalDefense() // 최종 방어력 계산하기
    {
        int baseDef = playerStats[(int)StatType.Def];
        int equipmentDef = 0;

        foreach (var item in Inventory.items)
        {
            if (item.IsEquipped)
            {
                var itemData = ShopItemManager.items.Find(x => x.IName == item.Name);
                if (itemData != null)
                {
                    equipmentDef += itemData.IDef;
                }
            }
        }
        return baseDef + equipmentDef;
    }
    public int GetHp() //HP 가져오기
    {
        return playerStats[(int)StatType.Hp];
    }

    public void SetHp(int value) //HP 설정하기
    {
        playerStats[(int)StatType.Hp] = value;
    }


    public static class Inventory
    {
        public static List<Item> items = new List<Item>();

        public static void ShowItems()
        {
            do
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine();
                Console.WriteLine("╔══════════════╗");
                Console.WriteLine("║   인벤토리   ║");
                Console.WriteLine("╚══════════════╝");
                Console.WriteLine();
                Console.ResetColor();
                if (items.Count == 0)
                {
                    Console.WriteLine("보유 중인 아이템이 없습니다.");
                    Console.WriteLine();
                }
                else
                {
                    foreach (var item in items)
                    {
                        var mark = item.GetEquippedMark();
                        Console.WriteLine($"{mark}{item.Name}");
                        Console.WriteLine();
                    }
                }
                Console.WriteLine("[1] 장착 관리");
                Console.WriteLine("[0] 나가기");
                Console.WriteLine();
                Console.Write(">> ");
                string input = Console.ReadLine();

                int num;
                bool isInt = int.TryParse(input, out num);

                if (isInt)
                {
                    switch (num)
                    {
                        case 0:
                            Menu.ShowMenu();
                            Loop.loop = false;
                            break;
                        case 1:
                            Inventory.ItemsEquipments();
                            break;

                        default:
                            Message.FailMessage();
                            break;
                    }
                }
                else
                {
                    Message.FailMessage();
                }
            } while (Loop.loop == true);
        }
        public static void ItemsEquipments()
        {
            do
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine();
                Console.WriteLine("╔══════════════════════════╗");
                Console.WriteLine("║  인벤토리 - 장착 관리    ║");
                Console.WriteLine("╚══════════════════════════╝");
                Console.WriteLine();
                Console.ResetColor();
                foreach (var item in items)
                {
                    string equippedMark = item.IsEquipped ? "[E] " : "";
                    Console.WriteLine($"[{Inventory.items.IndexOf(item) + 1}] {equippedMark}{item.Name}");
                    Console.WriteLine();
                }
                Console.WriteLine("[0] 나가기");
                Console.WriteLine();
                Console.Write(">> ");
                string input = Console.ReadLine();

                int num;
                bool isInt = int.TryParse(input, out num);

                if (isInt && num == 0)
                {
                    Menu.ShowMenu();
                    Loop.loop = false;
                    break;
                }
                else if (isInt && num > 0 && num <= Inventory.items.Count)
                {
                    var selectedItem = Inventory.items[num - 1];

                    if (selectedItem.IsEquipped)
                    {
                        Console.WriteLine($"{selectedItem.Name} 을(를) 해제하셨습니다.");
                        selectedItem.IsEquipped = false;
                        Thread.Sleep(800);
                    }
                    else
                    {
                        Console.WriteLine($"{selectedItem.Name} 을(를) 착용하셨습니다.");
                        selectedItem.IsEquipped = true;
                        Thread.Sleep(800);
                    }
                }
                else
                {
                    Message.FailMessage();
                }

            } while (Loop.loop);
        }
    }
    public class Item
    {
        public string Name;
        public bool IsEquipped;
        public int IPrice;
        public int IAtk;
        public int IDef;
        public bool IsPurchased;
        public string IDescription;
        public EquipmentType Type;

        public Item(string name, int price, int atk, int def, string description, EquipmentType type, bool isEquipped = false, bool isPurchased = false)
        {
            Name = name;
            IsEquipped = isEquipped;
            Name = name;
            IPrice = price;
            IAtk = atk;
            IDef = def;
            IDescription = description;
            Type = type;
            IsEquipped = isEquipped;
            IsPurchased = isPurchased;
        }
    }

    public class ItemSet
    {
        public string IName; // 아이템 이름
        public int IAtk; // 아이템 공격력
        public int IDef; // 아이템 방어력
        public string IDescription; // 아이템 설명
        public int IPrice; // 아이템 가격
        public bool IsPurchased; // 아이템 구매 유/무 
        public EquipmentType Type; // 아이템 타입 ( 무기 or 방어구 )
    }

    public enum EquipmentType
    {
        Weapon,
        Armor
    }

    public static class ShopItemManager
    {
        public static List<ItemSet> items = new List<ItemSet>();

        public static void InitializeShopItems()
        {
            items.Add(new ItemSet
            {
                IName = "로얄 반 레온 슈트",
                IAtk = 0,
                IDef = 5,
                IDescription = "북부의 사자 왕이 자주 입었던 한벌 옷이다.",
                IPrice = 800,
                IsPurchased = false,
                Type = EquipmentType.Armor
            });
            items.Add(new ItemSet
            {
                IName = "펜살리르 배틀메일",
                IAtk = 0,
                IDef = 10,
                IDescription = "전투용으로 안성맞춤인 한벌 옷이다.",
                IPrice = 1600,
                IsPurchased = false,
                Type = EquipmentType.Armor
            });
            items.Add(new ItemSet
            {
                IName = "앱솔랩스 슈트",
                IAtk = 0,
                IDef = 15,
                IDescription = "미지의 힘이 깃들어 있는 한벌 옷이다.",
                IPrice = 2500,
                IsPurchased = false,
                Type = EquipmentType.Armor
            });
            items.Add(new ItemSet
            {
                IName = "아케인셰이드 슈트",
                IAtk = 0,
                IDef = 25,
                IDescription = "세계 최고의 성능을 지닌 한벌 옷이다.",
                IPrice = 5000,
                IsPurchased = false,
                Type = EquipmentType.Armor
            });
            items.Add(new ItemSet
            {
                IName = "라 투핸더",
                IAtk = 7,
                IDef = 0,
                IDescription = "평범한 대장장이가 만들어낸 두손 검이다.",
                IPrice = 1000,
                IsPurchased = false,
                Type = EquipmentType.Weapon
            });
            items.Add(new ItemSet
            {
                IName = "저주받은 카이세리움",
                IAtk = 15,
                IDef = 0,
                IDescription = "저주받아 검게 타락한 두손 검이다.",
                IPrice = 2200,
                IsPurchased = false,
                Type = EquipmentType.Weapon
            });
            items.Add(new ItemSet
            {
                IName = "봉인된 제네시스 투핸드소드",
                IAtk = 25,
                IDef = 0,
                IDescription = "어둠에 잠식되어 힘이 봉인된 두손 검이다.",
                IPrice = 4000,
                IsPurchased = false,
                Type = EquipmentType.Weapon
            });
            items.Add(new ItemSet
            {
                IName = "데스티니 투핸드소드",
                IAtk = 60,
                IDef = 0,
                IDescription = "세계 최고의 성능을 지닌 두손 검이다.",
                IPrice = 10000,
                IsPurchased = false,
                Type = EquipmentType.Weapon
            });
        }
    }

    public class Loop
    {
        // 내가 지정한 범위 내의 코드를 반복한다 = true
        // 반복하지 않는다 = false

        public static bool loop = true;
    }
}
public class Menu
{
    public static void NameMeun()
    {
        // 플레이어가 이름을 작성하면 이름의 변수 명칭이 변경되게 만든다.
        do
        {
            Console.WriteLine();
            Console.WriteLine("당신의 닉네임을 작성해주세요.");
            Console.WriteLine();
            string input = Console.ReadLine();
            Console.WriteLine();
            Console.WriteLine($"당신의 닉네임을 {input} 으(로) 설정 하시겠습니까?");
            Console.WriteLine();
            Console.WriteLine("[1] 확인");
            Console.WriteLine("[2] 취소");
            Console.WriteLine();
            Console.Write(">> ");
            string input2 = Console.ReadLine();

            int num;
            bool isInt = int.TryParse(input2, out num);

            if (isInt)
            {
                switch (num)
                {
                    case 1:
                        GameData.player.playerName = input;
                        Menu.JopMenu();
                        Loop.loop = false;
                        break;
                    case 2:
                        Console.WriteLine();
                        Console.WriteLine("닉네임을 다시 작성해주세요.");
                        Console.WriteLine();
                        break;

                    default:
                        Message.FailMessage();
                        break;
                }
            }
            else
            {
                Message.FailMessage();
            }
        } while (Loop.loop);
    }


    public static void JopMenu()
    {
        // 플레이어가 직업 선택지에 맞는 번호를 입력하면 직업 변수 명칭이 변경되게 만든다.
        do
        {
            Console.WriteLine();
            Console.WriteLine("당신의 직업을 선택해주세요.");
            Console.WriteLine();
            Console.WriteLine("[1] 팔라딘");
            Console.WriteLine("[2] 섀도어");
            Console.WriteLine("[3] 카이저");
            Console.WriteLine();
            Console.Write(">> ");
            string input = Console.ReadLine();

            int job;
            bool isInt = int.TryParse(input, out job);

            if (isInt && job >= 1 && job <= 3)
            {
                GameData.player.playerJob = (JopType)job;

                Console.WriteLine();
                Console.WriteLine($"당신의 직업은 {JobNameKR.JobNames[GameData.player.playerJob]}입니다.");
                Console.WriteLine();
                Thread.Sleep(1000);
                Menu.ShowMenu();
                Loop.loop = false;
            }
            else
            {
                Message.FailMessage();
            }
        } while (Loop.loop);
    }


    public static void ShowMenu()
    {
        //-게임 시작시 간단한 소개 말과 마을에서 할 수 있는 행동을 알려줍니다.
        //-원하는 행동의 숫자를 타이핑하면 실행합니다. 
        //1 ~3 이외 입력시 -**잘못된 입력입니다** 출력

        {
            do
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("메이플 RPG에 오신 걸 환영합니다.");
                Console.WriteLine("원하는 행동을 선택할 수 있습니다.");
                Console.WriteLine();
                Console.WriteLine("[1] 상태창");
                Console.WriteLine("[2] 인벤토리");
                Console.WriteLine("[3] 상점");
                Console.WriteLine("[4] 휴식하기");
                Console.WriteLine();
                Console.WriteLine("원하시는 행동을 입력해주세요.");
                Console.WriteLine();
                Console.Write(">> ");

                string input = Console.ReadLine();

                int num;
                bool isInt = int.TryParse(input, out num);

                if (isInt)
                {
                    switch (num)
                    {
                        case 1:
                            Menu.Status();
                            Loop.loop = false;
                            break;
                        case 2:
                            Inventory.ShowItems();
                            Loop.loop = false;
                            break;
                        case 3:
                            Shop.ShopMenu();
                            Loop.loop = false;
                            break;
                        case 4:
                            Menu.HealMenu();
                            Loop.loop = false;
                            break;

                        default:
                            Message.FailMessage();
                            break;
                    }
                }
                else
                {
                    Message.FailMessage();
                }
            } while (Loop.loop);
        }
    }

    public static void Status()
    {
        do
        {

            // 공격력 계산
            int baseAtk = GameData.player.GetBaseStat(Player.StatType.Atk);
            int totalAtk = GameData.player.GetTotalAttack();
            int equipAtk = totalAtk - baseAtk;

            // 방어력 계산
            int baseDef = GameData.player.GetBaseStat(Player.StatType.Def);
            int totalDef = GameData.player.GetTotalDefense();
            int equipDef = totalDef - baseDef;

            Console.Clear();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("╔════════════╗");
            Console.WriteLine("║   상태창   ║");
            Console.WriteLine("╚════════════╝");
            Console.WriteLine();
            Console.ResetColor();
            Console.WriteLine($"Lv. {GameData.player.Level.ToString("D2")}");
            Console.WriteLine();
            Console.WriteLine($"{GameData.player.playerName}( {JobNameKR.JobNames[GameData.player.playerJob]} )");
            Console.WriteLine();
            Console.WriteLine($"공격력 : {totalAtk} (+{equipAtk})");
            Console.WriteLine();
            Console.WriteLine($"방어력 : {totalDef} (+{equipDef})");
            Console.WriteLine();
            Console.WriteLine($"체력 : {GameData.player.GetStat(Player.StatType.Hp)}");
            Console.WriteLine();
            Console.WriteLine($"Gold : {GameData.player.Gold}");
            Console.WriteLine();
            Console.WriteLine("[0] 나가기");
            Console.WriteLine();
            Console.WriteLine("원하시는 행동을 입력해주세요.");
            Console.WriteLine();
            Console.Write(">> ");
            string input = Console.ReadLine();

            int num;
            bool isInt = int.TryParse(input, out num);

            if (isInt)
            {
                switch (num)
                {
                    case 0:
                        Menu.ShowMenu();
                        Loop.loop = false;
                        break;

                    default:
                        Message.FailMessage();
                        break;
                }
            }
            else
            {
                Console.Clear();
                Message.FailMessage();
            }
        } while (Loop.loop);
    }


    public static void HealMenu()
    {
        do
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("╔══════════════╗");
            Console.WriteLine("║   휴식하기   ║");
            Console.WriteLine("╚══════════════╝");
            Console.WriteLine();
            Console.ResetColor();
            Console.WriteLine("500 G 를 지불하면 체력을 회복할 수 있습니다.");
            Console.WriteLine();
            Console.WriteLine($"보유 골드 :: {(GameData.player.Gold)}");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("[1] 휴식하기");
            Console.WriteLine("[0] 나가기");
            Console.WriteLine();
            Console.WriteLine("원하시는 행동을 입력해주세요.");
            Console.WriteLine();
            Console.Write(">> ");

            string input = Console.ReadLine();

            int num;
            bool isInt = int.TryParse(input, out num);

            if (isInt)
            {
                switch (num)
                {
                    case 0:
                        Menu.ShowMenu();
                        Loop.loop = false;
                        break;
                    case 1:
                        if (GameData.player.GetHp() < 100)
                        {
                            if (GameData.player.Gold >= 500)
                            {
                                GameData.player.Gold -= 500;
                                GameData.player.SetHp(100);
                                Console.WriteLine("휴식을 취해서 체력이 100으로 회복했습니다!");
                                Console.WriteLine();
                                Console.WriteLine($"보유 골드 :: {(GameData.player.Gold)}");
                                Thread.Sleep(800);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("당신은 500 G 를 보유하고 있지 않습니다!");
                                Console.WriteLine();
                                Console.WriteLine($"보유 골드 :: {(GameData.player.Gold)}");
                                Console.WriteLine();
                                Thread.Sleep(800);
                            }
                        }
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine("이미 체력이 충분합니다.");
                            Console.WriteLine();
                            Thread.Sleep(800);
                        }
                        break;

                    default:
                        Message.FailMessage();
                        break;
                }
            }
            else
            {
                Message.FailMessage();
            }

        } while (Loop.loop);
    }
}

public class Shop
{
    public static void ShopMenu()
    {
        do
        {

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine();
            Console.WriteLine("╔════════════╗");
            Console.WriteLine("║    상점    ║");
            Console.WriteLine("╚════════════╝");
            Console.WriteLine();
            Console.ResetColor();
            Console.WriteLine("필요한 아이템을 얻을 수 있는 상점입니다.");
            Console.WriteLine();
            Console.WriteLine($"보유 골드 :: {(GameData.player.Gold)}");
            Console.WriteLine();
            Console.WriteLine();

            //-보유중인 골드와 아이템의 정보,가격이 표시됩니다.
            //-아이템 정보 오른쪽에는 가격이 표시가 됩니다.
            //- 이미 구매를 완료한 아이템이라면** 구매완료**로 표시됩니다.

            //상점에서 판매 중인 아이템을 출력하기 위해서는 리스트를 작성해야 한다.
            //리스트를 작성하고 출력하는 코드는 인벤토리에서 이미 작성했기에 그 틀을 기초로 잡고 만들면 될 것 같다.

            foreach (var item in ShopItemManager.items)
            {
                string stat = item.Type == EquipmentType.Weapon ? $"공격력 +{item.IAtk}" : $"방어력 +{item.IDef}";
                string price = item.IsPurchased ? "구매완료" : $"{item.IPrice} G";
                Console.WriteLine($"※ {item.IName} | {stat} | {item.IDescription} | {price} |");
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("[1] 아이템 구매");
            Console.WriteLine("[2] 아이템 판매");
            Console.WriteLine("[0] 나가기");
            Console.WriteLine();
            Console.WriteLine("원하시는 행동을 입력해주세요.");
            Console.WriteLine();
            Console.Write(">> ");

            string input = Console.ReadLine();

            int num;
            bool isInt = int.TryParse(input, out num);

            if (isInt)
            {
                switch (num)
                {
                    case 0:
                        Menu.ShowMenu();
                        Loop.loop = false;
                        break;
                    case 1:
                        Shop.ShopBuy();
                        Loop.loop = false;
                        break;
                    case 2:
                        Shop.ShopSell();
                        Loop.loop = false;
                        break;

                    default:
                        Message.FailMessage();
                        break;
                }
            }
            else
            {
                Message.FailMessage();
            }

        } while (Loop.loop);
    }

    public static void ShopBuy()
    {
        do
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine();
            Console.WriteLine("╔════════════╗");
            Console.WriteLine("║    구매    ║");
            Console.WriteLine("╚════════════╝");
            Console.WriteLine();
            Console.ResetColor();
            Console.WriteLine("원하는 물건을 구매하실 수 있습니다.");
            Console.WriteLine();
            Console.WriteLine($"보유 골드 :: {(GameData.player.Gold)}");
            Console.WriteLine();
            Console.WriteLine();

            foreach (var item in ShopItemManager.items)
            {
                string stat = item.Type == EquipmentType.Weapon ? $"공격력 +{item.IAtk}" : $"방어력 +{item.IDef}";
                string price = item.IsPurchased ? "구매완료" : $"{item.IPrice} G";
                Console.WriteLine($"[{ShopItemManager.items.IndexOf(item) + 1}] {item.IName} | {stat} | {item.IDescription} | {price} |");
                Console.WriteLine();
            }

            Console.WriteLine("[0] 나가기");
            Console.WriteLine();
            Console.WriteLine("구매할 아이템의 번호를 입력해주세요.");
            Console.WriteLine();
            Console.Write(">> ");

            string input = Console.ReadLine();
            int num;
            bool isInt = int.TryParse(input, out num);

            if (isInt && num == 0)
            {
                Menu.ShowMenu();
                Loop.loop = false;
                break;
            }
            else if (isInt && num > 0 && num <= ShopItemManager.items.Count)
            {
                var selectedItem = ShopItemManager.items[num - 1];

                if (selectedItem.IsPurchased)
                {
                    Console.WriteLine("이미 구매하신 아이템 입니다.");
                }
                else
                {
                    if (GameData.player.Gold >= selectedItem.IPrice)
                    {
                        GameData.player.Gold -= selectedItem.IPrice;
                        selectedItem.IsPurchased = true;
                        Console.WriteLine($"{selectedItem.IName}을(를) 성공적으로 구매했습니다 !");
                        var newItem = new Item(
                        name: selectedItem.IName,
                        price: selectedItem.IPrice,
                        atk: selectedItem.IAtk,
                        def: selectedItem.IDef,
                        description: selectedItem.IDescription,
                        type: selectedItem.Type
                    );
                        Player.Inventory.items.Add(newItem);
                        Thread.Sleep(800);
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine($"구매에 필요한 Gold가 {selectedItem.IPrice - GameData.player.Gold} 만큼 부족합니다.");
                        Console.WriteLine();
                        Thread.Sleep(800);
                    }
                }
            }
            else
            {
                Message.FailMessage();
            }
        } while (Loop.loop);
    }
    public static void ShopSell()
    {
        do
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine();
            Console.WriteLine("╔════════════╗");
            Console.WriteLine("║    판매    ║");
            Console.WriteLine("╚════════════╝");
            Console.WriteLine();
            Console.ResetColor();
            Console.WriteLine("원하는 물건을 판매하실 수 있습니다.");
            Console.WriteLine();
            Console.WriteLine($"보유 골드 :: {(GameData.player.Gold)}");
            Console.WriteLine();
            Console.WriteLine();

            foreach (var item in items)
            {
                var SellItem = ShopItemManager.items.FirstOrDefault(x => x.IName == item.Name);
                var mark = item.GetEquippedMark();

                Console.WriteLine($"[{items.IndexOf(item) + 1}] {mark}{SellItem.IName} | 원가 {SellItem.IPrice} G | 판매 값 {SellItem.IPrice * 0.85} G");
                Console.WriteLine();
            }

            Console.WriteLine("[0] 나가기");
            Console.WriteLine();
            Console.WriteLine("판매할 아이템의 번호를 입력해주세요.");
            Console.WriteLine();
            Console.Write(">> ");

            string input = Console.ReadLine();
            int num;
            bool isInt = int.TryParse(input, out num);

            if (isInt && num == 0)
            {
                Menu.ShowMenu();
                Loop.loop = false;
                break;
            }
            else if (isInt && num > 0 && num <= items.Count)
            {
                var selectedItem = Player.Inventory.items[num - 1];
                var shopItem = ShopItemManager.items.FirstOrDefault(x => x.IName == selectedItem.Name);

                if (selectedItem.IsEquipped)
                {
                    Shop.SellWarning(selectedItem);
                }
                else
                {
                    Shop.SellWarning(selectedItem);
                }
            }
        } while (Loop.loop);
    }
    public static void SellItem(Item selectedItem)
    {
        GameData.player.Gold += (int)(selectedItem.IPrice * 0.85);

        var playerItem = Player.Inventory.items.FirstOrDefault(x => x.Name == selectedItem.Name);
        var shopItem = ShopItemManager.items.FirstOrDefault(x => x.IName == selectedItem.Name);

        if (playerItem != null && shopItem != null)
        {
            Player.Inventory.items.Remove(playerItem);
            shopItem.IsPurchased = false;
        }

        Console.WriteLine();
        Console.WriteLine("판매에 성공하셨습니다!");
        Console.WriteLine();
        Console.WriteLine($"보유 골드 : {GameData.player.Gold} G");
        Console.WriteLine();
        Thread.Sleep(800);
    }
    public static void SellWarning(Item selectedItem)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine();
        Console.WriteLine("╔════════════╗");
        Console.WriteLine("║    경고    ║");
        Console.WriteLine("╚════════════╝");
        Console.WriteLine();
        Console.ResetColor();
        Console.WriteLine("정말 이 아이템을 판매 하시겠습니까?");
        Console.WriteLine();
        Console.WriteLine($"[선택된 장비] {selectedItem.Name}");
        Console.WriteLine();
        Console.WriteLine($"- 원가      : {selectedItem.IPrice} G");
        Console.WriteLine();
        Console.WriteLine($"- 판매가    : {(int)(selectedItem.IPrice * 0.85)} G");
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("[1] 판매");
        Console.WriteLine("[0] 뒤로 가기");
        Console.WriteLine();
        Console.WriteLine("원하시는 행동을 입력해주세요.");
        Console.WriteLine();
        Console.Write(">> ");

        string input = Console.ReadLine();
        int num;
        bool isInt = int.TryParse(input, out num);

        switch (num)
        {
            case 0:
                Shop.ShopSell();
                break;
            case 1:
                Shop.SellItem(selectedItem);
                break;

            default:
                Message.FailMessage();
                break;
        }
    }
}

    public class Message
    {
        public static void FailMessage()
        {
            Console.WriteLine();
            Console.WriteLine("잘못된 입력입니다.");
            Console.WriteLine();
            Thread.Sleep(800);
        }
    }
    public enum JopType
    {
        None = 0, // 기본값
        Paladin = 1, //팔라딘
        Shadower = 2, //섀도어
        Kaiser = 3  //카이저
    }
    public static class ItemUtils
    {
        public static string GetEquippedMark(this Item item)
        {
            return item.IsEquipped ? "[E] " : "";
        }
    }

    public static class JobNameKR
    {
        public static readonly Dictionary<JopType, string> JobNames = new Dictionary<JopType, string>()
    {
        { JopType.Paladin, "팔라딘" },
        { JopType.Shadower, "섀도어" },
        { JopType.Kaiser, "카이저" }
    };
    }
