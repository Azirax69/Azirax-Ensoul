namespace Azirax_Amumu
{
   
    using System;
    using System.Linq;
    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;
    using EnsoulSharp.SDK.MenuUI.Values;
    using EnsoulSharp.SDK.Prediction;
    using EnsoulSharp.SDK.Utility;
    using Color = System.Drawing.Color;

    public class Program
    {
        private static Menu MainMenu;

        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell R;


        private static void Main(string[] args)
        {
            
            GameEvent.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad()
        {
           
            if (ObjectManager.Player.CharacterName != "Amumu")
            {
                return;
            }


            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 300);
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 550);
            Q.SetSkillshot(250f, 90f, 2000f, true, SkillshotType.Line);


            


            MainMenu = new Menu("aziraxamumu", "Azirax Amumu", true);

            
            var comboMenu = new Menu("Combo", "Combo Config");
            comboMenu.Add(new MenuBool("comboQ", "Use Q", true));
            comboMenu.Add(new MenuBool("comboW", "Use W", true));
            comboMenu.Add(new MenuBool("comboE", "Use E", true));
            comboMenu.Add(new MenuSlider("comboR", "Min Enemies for R", 3, 1, 5));
            MainMenu.Add(comboMenu);

            var laneclearMenu = new Menu("Lane Clear", "LaneClear");
            laneclearMenu.Add(new MenuBool("clearQ", "Use Q", true));
            laneclearMenu.Add(new MenuBool("clearW", "Use W", true));
            laneclearMenu.Add(new MenuBool("clearE", "Use E", true));
            laneclearMenu.Add(new MenuSlider("ManaCheck", "Don't LaneClear if mana < %", 0, 0, 100));
            MainMenu.Add(laneclearMenu);

            var jungleclearMenu = new Menu("Jungle Clear", "JungleClear");
            jungleclearMenu.Add(new MenuBool("jungleQ", "Use Q", true));
            jungleclearMenu.Add(new MenuBool("jungleW", "Use W", true));
            jungleclearMenu.Add(new MenuBool("jungleE", "Use E", true));
            jungleclearMenu.Add(new MenuSlider("ManaCheck", "Don't LaneClear if mana < %", 0, 0, 100));
            MainMenu.Add(jungleclearMenu);

            var drawMenu = new Menu("Draw", "Draw Settings");
            drawMenu.Add(new MenuBool("drawQ", "Draw Q Range", true));
            drawMenu.Add(new MenuBool("drawW", "Draw W Range", true));
            drawMenu.Add(new MenuBool("drawE", "Draw E Range", true));
            drawMenu.Add(new MenuBool("drawR", "Draw R Range", true));
            MainMenu.Add(drawMenu);


            Chat.Print("Azirax Amumu, Any problem, report in my discord");

            MainMenu.Attach();
            
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
        }

        private static void Combo()
        {

            var target = TargetSelector.GetTarget(Q.Range);

            if (target == null)  
            {
                return;
            }
            if (MainMenu["Combo"]["comboQ"].GetValue<MenuBool>().Enabled && Q.IsReady() && target.IsValidTarget(Q.Range) && Q.GetPrediction(target).Hitchance >= HitChance.Medium)
            {
                Q.CastIfHitchanceEquals(target, HitChance.Medium);
            }
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
            {
                if (MainMenu["Combo"]["comboW"].GetValue<MenuBool>().Enabled && W.IsReady() && ObjectManager.Get<AIHeroClient>().Any(hero => hero.IsValidTarget(W.Range)))
                {
                    W.Cast();
                }
            }
            if (MainMenu["Combo"]["comboE"].GetValue<MenuBool>().Enabled && E.IsReady())
            {
                E.Cast(target);
            }
            var enemyCount = ObjectManager.Get<AIHeroClient>().Count(e => e.IsValidTarget(R.Range));
            if (MainMenu["Combo"]["comboR"].GetValue<MenuBool>().Enabled && R.IsReady())
            {
                R.Cast();
            }

        }

        private static void Clear()
        {
            var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range) && x.IsMinion());
            if (MainMenu["Lane Clear"]["clearQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
            {
                foreach (var minion in minions)
                {
                    if (minion.IsValidTarget())
                    {
                        Q.GetPrediction(minion);
                        Q.CastIfHitchanceEquals(minion, HitChance.Medium);
                    }
                }
            }
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
            {
                if (MainMenu["Lane Clear"]["clearW"].GetValue<MenuBool>().Enabled && W.IsReady() && ObjectManager.Get<AIMinionClient>().Any(minion => minion.IsValidTarget(W.Range)))

                {
                    W.Cast();
                }
            }
            if (MainMenu["Lane Clear"]["clearE"].GetValue<MenuBool>().Enabled && E.IsReady())
            {
                foreach (var minion in minions)
                {
                    if (minion.IsValidTarget())
                    {
                        E.CastOnUnit(minion);
                    }

                }
            }

        }


        static void JungleClear()
        {
            var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(Q.Range));
            if (MainMenu["Jungle Clear"]["jungleQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
            {
                foreach (var minion in mobs)
                {
                    if (minion.IsValidTarget())
                    {
                        Q.GetPrediction(minion);
                        Q.CastIfHitchanceEquals(minion, HitChance.High);
                    }
                }
            }
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
            {
                if (MainMenu["Lane Clear"]["jungleW"].GetValue<MenuBool>().Enabled && W.IsReady() && ObjectManager.Get<AIMinionClient>().Any(minion => minion.IsValidTarget(W.Range)))

                {
                    W.Cast();
                }
            }
            if (MainMenu["Lane Clear"]["jungleE"].GetValue<MenuBool>().Enabled && E.IsReady())
            {
                foreach (var minion in mobs)
                {
                    if (minion.IsValidTarget())
                    {
                        E.CastOnUnit(minion);
                    }

                }
            }

        }

        private static void OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.LaneClear:
                    Clear();
                    JungleClear();
                    break;
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (MainMenu["Draw"]["drawQ"].GetValue<MenuBool>().Enabled)
            {      
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.OrangeRed);
            }
            if (MainMenu["Draw"]["drawW"].GetValue<MenuBool>().Enabled)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, Color.OrangeRed);
            }
            if (MainMenu["Draw"]["drawE"].GetValue<MenuBool>().Enabled)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, Color.OrangeRed);
            }
            if (MainMenu["Draw"]["drawR"].GetValue<MenuBool>().Enabled)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, Color.OrangeRed);
            }

        }
    }
}
