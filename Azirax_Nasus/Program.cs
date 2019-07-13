namespace Nasus
{
    using System;
    using System.Linq;
    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;
    using EnsoulSharp.SDK.MenuUI.Values;
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
            
            if (ObjectManager.Player.CharacterName != "Nasus")
            {
                return;
            }


            Q = new Spell(SpellSlot.Q, 350f);

            W = new Spell(SpellSlot.W, 600f);

            E = new Spell(SpellSlot.E, 650f);

            R = new Spell(SpellSlot.R, 20f); 


            
            MainMenu = new Menu("aziraxnasus", "Azirax Nasus", true);

            
            var comboMenu = new Menu("Combo", "Combo Config");
            comboMenu.Add(new MenuBool("comboQ", "Use Q", true));
            comboMenu.Add(new MenuBool("comboW", "Use W", true));
            comboMenu.Add(new MenuBool("comboE", "Use E", true));
            comboMenu.Add(new MenuBool("comboR", "Use R", true));
            MainMenu.Add(comboMenu);

            var laneclearMenu = new Menu("Clear", "Lane Clear");
            laneclearMenu.Add(new MenuBool("clearQ", "Use Q", true));
            MainMenu.Add(laneclearMenu);

            var lasthitMenu = new Menu("Farm", "Farm");
            lasthitMenu.Add(new MenuBool("farmQ", "Use Q", true));
            MainMenu.Add(lasthitMenu);

            var drawMenu = new Menu("Draw", "Draw Settings");
            drawMenu.Add(new MenuBool("drawQ", "Draw Q Range"));
            drawMenu.Add(new MenuBool("drawW", "Draw W Range"));
            drawMenu.Add(new MenuBool("drawE", "Draw E Range"));
            MainMenu.Add(drawMenu);

            Chat.PrintChat("Azirax Nasus, Any problem, report in my discord = Azirax#5495");


            MainMenu.Attach();
           
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
        }

        // ainda estou tentando corrigir os combos by: Azirax#5495
        private static void Combo()
        {

            if (MainMenu["Combo"]["comboW"].GetValue<MenuBool>().Enabled && W.IsReady())
            {

                var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                target = TargetSelector.GetTarget(W.Range);

                if (target != null && target.IsValidTarget(W.Range))
                {

                    W.CastOnUnit(target);
                }

            }

            if (MainMenu["Combo"]["comboE"].GetValue<MenuBool>().Enabled && E.IsReady())
            {

                var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
                target = TargetSelector.GetTarget(E.Range);

                if (target != null && target.IsValidTarget(E.Range))
                {

                    E.CastOnUnit(target);
                }
            }

            if (MainMenu["Combo"]["comboQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
            {

                var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                target = TargetSelector.GetTarget(Q.Range);

                if (target != null && target.IsValidTarget(Q.Range))
                {

                    Q.CastOnUnit(target);
                }

            }

            if (MainMenu["Combo"]["comboR"].GetValue<MenuBool>().Enabled && R.IsReady())
            if (ObjectManager.Player.CountEnemyHeroesInRange(750) >= 1)
            {
                R.Cast();
            }

        }

        private static void Clear()
        {

            var qminions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(ObjectManager.Player.GetRealAutoAttackRange()) && x.IsMinion()).Cast<AIBaseClient>().ToList();

            foreach (AIBaseClient minion in qminions)
            {
                if (Q.IsReady())
                {
                    Q.Cast();
                    // Thank you Putão com Tesão for helping me out on the nasus farm.
                }
            }


        }

        private static void Farm()
        {

            var qminions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(ObjectManager.Player.GetRealAutoAttackRange()) && x.IsMinion()).Cast<AIBaseClient>().ToList();

            foreach (AIBaseClient minion in qminions)
            {
                if (Q.IsReady())
                {
                    Q.Cast();
                    // Thank you Putão com Tesão for helping me out on the nasus farm.
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
                    break;
                case OrbwalkerMode.LastHit:
                    Farm();
                    break;
            }
        }

        private static void OnDraw(EventArgs args)
        {
            
            if (MainMenu["isDead"].GetValue<MenuBool>().Enabled)
            {
                if (ObjectManager.Player.IsDead)
                {
                    return;
                }
            }

            // draw Q Range
            if (MainMenu["Draw"]["drawQ"].GetValue<MenuBool>().Enabled)
            {
                
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.Black);
            }

            // draw E Range
            if (MainMenu["Draw"]["drawE"].GetValue<MenuBool>().Enabled)
            {

                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Black);
            }

        }
    }
}
