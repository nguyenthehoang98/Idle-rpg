using UnityEngine;
using UnityEngine.UI;

namespace _TDS.UI
{
    public enum UiPreviewPage
    {
        Home,
        Gameplay,
        Reward,
        Shop,
        Augment,
        Result,
    }

    public sealed class UiPreviewScreen : UiScreenRoot
    {
        private static readonly Color Scrim = new Color(0.02f, 0.05f, 0.1f, 0.82f);
        private UiPreviewPage currentPage;

        protected override void BuildScreen()
        {
            ShowPage(UiPreviewPage.Home);
        }

        public void ShowPage(UiPreviewPage page)
        {
            currentPage = page;
            ClearScreen();
            switch (page)
            {
                case UiPreviewPage.Home: BuildHome(); break;
                case UiPreviewPage.Gameplay: BuildGameplay(); break;
                case UiPreviewPage.Reward: BuildReward(); break;
                case UiPreviewPage.Shop: BuildShop(); break;
                case UiPreviewPage.Augment: BuildAugment(); break;
                case UiPreviewPage.Result: BuildResult(); break;
            }
        }

        private void BuildHome()
        {
            CreateBackground();
            CreateHeader("IDLE / CIRCUIT", "RUN SELECT", UiPreviewPage.Home);

            UiPanel hero = CreatePanel("HeroCard", ScreenContent, UiPanelTone.Surface);
            SetRect(hero.GetComponent<RectTransform>(), new Vector2(0.06f, 0.69f), new Vector2(0.94f, 0.87f));
            CreateText("Eyebrow", hero.transform, "ENERGY CIRCUIT", UiTextRole.Caption);
            SetRect(hero.transform.Find("Eyebrow") as RectTransform, new Vector2(0.06f, 0.65f), new Vector2(0.94f, 0.9f));
            CreateText("Title", hero.transform, "BUILD YOUR RUN", UiTextRole.Display, TextAnchor.MiddleCenter);
            SetRect(hero.transform.Find("Title") as RectTransform, new Vector2(0.06f, 0.27f), new Vector2(0.94f, 0.7f));
            CreateText("Body", hero.transform, "Place heroes and items on the circuit.\nTrigger Overdrive. Survive the boss.", UiTextRole.Body, TextAnchor.MiddleCenter);
            SetRect(hero.transform.Find("Body") as RectTransform, new Vector2(0.06f, 0.02f), new Vector2(0.94f, 0.3f));

            CreateText("LevelLabel", ScreenContent, "SELECT LEVEL", UiTextRole.Heading);
            SetRect(ScreenContent.Find("LevelLabel") as RectTransform, new Vector2(0.06f, 0.6f), new Vector2(0.94f, 0.66f));
            for (int i = 0; i < 3; i++)
            {
                UiButton level = CreateButton($"Level{i + 1}", ScreenContent, $"LEVEL {i + 1:00}\nSTART", i == 0 ? UiButtonTone.Primary : UiButtonTone.Secondary);
                RectTransform rect = level.GetComponent<RectTransform>();
                float min = 0.06f + i * 0.3f;
                SetRect(rect, new Vector2(min, 0.48f), new Vector2(min + 0.26f, 0.57f));
                int page = i;
                level.Button.onClick.AddListener(() => ShowPage(page == 0 ? UiPreviewPage.Gameplay : UiPreviewPage.Gameplay));
            }

            UiButton continueButton = CreateButton("Continue", ScreenContent, "CONTINUE RUN", UiButtonTone.Primary, UiTextRole.Heading);
            SetRect(continueButton.GetComponent<RectTransform>(), new Vector2(0.06f, 0.24f), new Vector2(0.94f, 0.34f));
            continueButton.Button.onClick.AddListener(() => ShowPage(UiPreviewPage.Gameplay));
            CreateText("Hint", ScreenContent, "Progress is saved automatically", UiTextRole.Caption, TextAnchor.MiddleCenter);
            SetRect(ScreenContent.Find("Hint") as RectTransform, new Vector2(0.06f, 0.15f), new Vector2(0.94f, 0.2f));
            CreateBottomNav(UiPreviewPage.Home);
        }

        private void BuildGameplay()
        {
            CreateBackground();
            CreateHeader("LEVEL 01", "WAVE 03 / 05", UiPreviewPage.Gameplay);
            CreateMetric("Gold", "GOLD", "240", 0.06f, 0.88f, 0.28f);
            CreateMetric("Status", "STATUS", "OVERDRIVE READY", 0.32f, 0.88f, 0.62f);
            UiBadge overdrive = CreateBadge("OverdriveBadge", ScreenContent, "OVERDRIVE", UiBadgeTone.Accent);
            SetRect(overdrive.GetComponent<RectTransform>(), new Vector2(0.66f, 0.88f), new Vector2(0.94f, 0.94f));

            UiPanel battlefield = CreatePanel("Battlefield", ScreenContent, UiPanelTone.Background);
            SetRect(battlefield.GetComponent<RectTransform>(), new Vector2(0.06f, 0.43f), new Vector2(0.94f, 0.84f));
            CreateText("BattleLabel", battlefield.transform, "AUTO COMBAT", UiTextRole.Heading, TextAnchor.MiddleCenter);
            SetRect(battlefield.transform.Find("BattleLabel") as RectTransform, new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.6f));
            CreateText("BattleHint", battlefield.transform, "Watch the circuit charge your heroes", UiTextRole.Caption, TextAnchor.MiddleCenter);
            SetRect(battlefield.transform.Find("BattleHint") as RectTransform, new Vector2(0.1f, 0.34f), new Vector2(0.9f, 0.45f));

            UiPanel circuit = CreatePanel("Circuit", ScreenContent, UiPanelTone.Surface);
            SetRect(circuit.GetComponent<RectTransform>(), new Vector2(0.06f, 0.2f), new Vector2(0.94f, 0.39f));
            CreateText("CircuitTitle", circuit.transform, "ENERGY CIRCUIT", UiTextRole.Heading);
            SetRect(circuit.transform.Find("CircuitTitle") as RectTransform, new Vector2(0.05f, 0.65f), new Vector2(0.5f, 0.95f));
            for (int i = 0; i < 8; i++)
            {
                UiPanel slot = CreatePanel($"Slot{i + 1:00}", circuit.transform, i == 3 ? UiPanelTone.Accent : UiPanelTone.Elevated);
                float min = 0.04f + i * 0.12f;
                SetRect(slot.GetComponent<RectTransform>(), new Vector2(min, 0.14f), new Vector2(min + 0.1f, 0.58f));
                CreateText("SlotLabel", slot.transform, i == 0 ? "GEN" : i < 3 ? $"H{i}" : "·", UiTextRole.Label, TextAnchor.MiddleCenter);
                SetRect(slot.transform.Find("SlotLabel") as RectTransform, Vector2.zero, Vector2.one);
            }

            UiButton shop = CreateButton("OpenShop", ScreenContent, "OPEN SHOP", UiButtonTone.Primary, UiTextRole.Heading);
            SetRect(shop.GetComponent<RectTransform>(), new Vector2(0.06f, 0.07f), new Vector2(0.58f, 0.15f));
            shop.Button.onClick.AddListener(() => ShowPage(UiPreviewPage.Reward));
            UiButton speed = CreateButton("Speed", ScreenContent, "1X", UiButtonTone.Secondary);
            SetRect(speed.GetComponent<RectTransform>(), new Vector2(0.64f, 0.07f), new Vector2(0.94f, 0.15f));
            CreateBottomNav(UiPreviewPage.Gameplay);
        }

        private void BuildReward()
        {
            CreateBackground();
            UiPanel card = CreatePanel("RewardCard", ScreenContent, UiPanelTone.Surface);
            SetRect(card.GetComponent<RectTransform>(), new Vector2(0.06f, 0.22f), new Vector2(0.94f, 0.78f));
            CreateText("Eyebrow", card.transform, "WAVE 03 CLEARED", UiTextRole.Caption, TextAnchor.MiddleCenter);
            SetRect(card.transform.Find("Eyebrow") as RectTransform, new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.9f));
            CreateText("Title", card.transform, "REWARD", UiTextRole.Display, TextAnchor.MiddleCenter);
            SetRect(card.transform.Find("Title") as RectTransform, new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.8f));
            CreateText("RewardValue", card.transform, "+120 GOLD\n+80 EXP", UiTextRole.Heading, TextAnchor.MiddleCenter);
            SetRect(card.transform.Find("RewardValue") as RectTransform, new Vector2(0.08f, 0.43f), new Vector2(0.92f, 0.6f));
            UiButton shop = CreateButton("RewardShop", card.transform, "GO TO SHOP", UiButtonTone.Primary, UiTextRole.Heading);
            SetRect(shop.GetComponent<RectTransform>(), new Vector2(0.1f, 0.23f), new Vector2(0.9f, 0.34f));
            shop.Button.onClick.AddListener(() => ShowPage(UiPreviewPage.Shop));
            UiButton augment = CreateButton("RewardAugment", card.transform, "CHOOSE AUGMENT", UiButtonTone.Secondary);
            SetRect(augment.GetComponent<RectTransform>(), new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.21f));
            augment.Button.onClick.AddListener(() => ShowPage(UiPreviewPage.Augment));
        }

        private void BuildShop()
        {
            CreateBackground();
            CreateHeader("SHOP", "SPEND GOLD / SHAPE YOUR BUILD", UiPreviewPage.Reward);
            CreateMetric("ShopGold", "RUN GOLD", "240", 0.06f, 0.87f, 0.4f);
            CreateMetric("Refresh", "REFRESH", "1 LEFT", 0.44f, 0.87f, 0.94f);
            UiDivider shopDivider = CreateDivider("ShopDivider", ScreenContent, UiDividerTone.Accent);
            SetRect(shopDivider.GetComponent<RectTransform>(), new Vector2(0.06f, 0.84f), new Vector2(0.94f, 0.845f));
            CreateOffer("Offer1", "GENERATOR", "Energy / Pulse", "40 GOLD", UiButtonTone.Primary, 0.06f, 0.62f);
            CreateOffer("Offer2", "AETHER", "DPS / Critical", "80 GOLD", UiButtonTone.Secondary, 0.06f, 0.4f);
            CreateOffer("Offer3", "BATTERY", "Energy / Defense", "60 GOLD", UiButtonTone.Secondary, 0.06f, 0.18f);
            UiButton next = CreateButton("NextWave", ScreenContent, "START NEXT WAVE", UiButtonTone.Primary, UiTextRole.Heading);
            SetRect(next.GetComponent<RectTransform>(), new Vector2(0.06f, 0.07f), new Vector2(0.94f, 0.15f));
            next.Button.onClick.AddListener(() => ShowPage(UiPreviewPage.Gameplay));
        }

        private void BuildAugment()
        {
            CreateBackground();
            CreateHeader("AUGMENT", "CHOOSE ONE / KEEP UNTIL RUN END", UiPreviewPage.Reward);
            CreateOffer("Augment1", "OVERCHARGE", "Every fourth pulse grants extra stack.", "ENERGY", UiButtonTone.Primary, 0.06f, 0.64f);
            CreateOffer("Augment2", "SCAVENGER", "Elite and boss rewards grant more gold.", "ECONOMY", UiButtonTone.Secondary, 0.06f, 0.43f);
            CreateOffer("Augment3", "CHAIN REACTION", "Overdrive kills can trigger an explosion.", "COMBAT", UiButtonTone.Secondary, 0.06f, 0.22f);
        }

        private void BuildResult()
        {
            CreateBackground();
            UiPanel card = CreatePanel("ResultCard", ScreenContent, UiPanelTone.Surface);
            SetRect(card.GetComponent<RectTransform>(), new Vector2(0.06f, 0.28f), new Vector2(0.94f, 0.72f));
            CreateText("Result", card.transform, "VICTORY", UiTextRole.Display, TextAnchor.MiddleCenter);
            SetRect(card.transform.Find("Result") as RectTransform, new Vector2(0.05f, 0.68f), new Vector2(0.95f, 0.9f));
            CreateText("Summary", card.transform, "BOSS DEFEATED\n+340 GOLD   +180 EXP", UiTextRole.Body, TextAnchor.MiddleCenter);
            SetRect(card.transform.Find("Summary") as RectTransform, new Vector2(0.05f, 0.48f), new Vector2(0.95f, 0.65f));
            UiButton next = CreateButton("NextLevel", card.transform, "NEXT LEVEL", UiButtonTone.Primary, UiTextRole.Heading);
            SetRect(next.GetComponent<RectTransform>(), new Vector2(0.1f, 0.22f), new Vector2(0.9f, 0.34f));
            next.Button.onClick.AddListener(() => ShowPage(UiPreviewPage.Home));
        }

        private void CreateBackground()
        {
            UiPanel background = CreatePanel("Background", ScreenContent, UiPanelTone.Background);
            SetRect(background.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        }

        private void CreateHeader(string title, string subtitle, UiPreviewPage backPage)
        {
            UiPanel header = CreatePanel("Header", ScreenContent, UiPanelTone.Surface);
            SetRect(header.GetComponent<RectTransform>(), new Vector2(0.04f, 0.9f), new Vector2(0.96f, 0.98f));
            CreateText("Title", header.transform, title, UiTextRole.Heading);
            SetRect(header.transform.Find("Title") as RectTransform, new Vector2(0.05f, 0.2f), new Vector2(0.62f, 0.8f));
            CreateText("Subtitle", header.transform, subtitle, UiTextRole.Caption, TextAnchor.MiddleRight);
            SetRect(header.transform.Find("Subtitle") as RectTransform, new Vector2(0.45f, 0.2f), new Vector2(0.94f, 0.8f));
            if (currentPage == UiPreviewPage.Home) return;

            UiButton back = CreateButton("Back", header.transform, "‹", UiButtonTone.Ghost, UiTextRole.Heading);
            SetRect(back.GetComponent<RectTransform>(), new Vector2(0.82f, 0.1f), new Vector2(0.98f, 0.9f));
            back.Button.onClick.AddListener(() => ShowPage(backPage));
        }

        private void CreateMetric(string name, string label, string value, float minX, float minY, float maxX)
        {
            UiPanel metric = CreatePanel(name, ScreenContent, UiPanelTone.Surface);
            SetRect(metric.GetComponent<RectTransform>(), new Vector2(minX, minY), new Vector2(maxX, minY + 0.06f));
            CreateText("Label", metric.transform, $"{label}  {value}", UiTextRole.Label, TextAnchor.MiddleCenter);
            SetRect(metric.transform.Find("Label") as RectTransform, Vector2.zero, Vector2.one);
        }

        private void CreateOffer(
            string name,
            string title,
            string description,
            string priceOrTag,
            UiButtonTone tone,
            float minX,
            float minY)
        {
            UiPanel card = CreatePanel(name, ScreenContent, UiPanelTone.Surface);
            SetRect(card.GetComponent<RectTransform>(), new Vector2(minX, minY), new Vector2(0.94f, minY + 0.17f));
            CreateText("Title", card.transform, title, UiTextRole.Heading);
            SetRect(card.transform.Find("Title") as RectTransform, new Vector2(0.05f, 0.57f), new Vector2(0.62f, 0.88f));
            CreateText("Description", card.transform, description, UiTextRole.Caption);
            SetRect(card.transform.Find("Description") as RectTransform, new Vector2(0.05f, 0.18f), new Vector2(0.62f, 0.52f));
            UiButton action = CreateButton("Action", card.transform, priceOrTag, tone, UiTextRole.Label);
            SetRect(action.GetComponent<RectTransform>(), new Vector2(0.68f, 0.25f), new Vector2(0.95f, 0.75f));
        }

        private void CreateBottomNav(UiPreviewPage activePage)
        {
            UiPanel nav = CreatePanel("BottomNav", ScreenContent, UiPanelTone.Surface);
            SetRect(nav.GetComponent<RectTransform>(), new Vector2(0.04f, 0.01f), new Vector2(0.96f, 0.06f));
            string[] labels = { "HOME", "RUN", "COLLECTION" };
            for (int i = 0; i < labels.Length; i++)
            {
                UiButton button = CreateButton(labels[i], nav.transform, labels[i], i == 0 ? UiButtonTone.Primary : UiButtonTone.Ghost);
                float min = 0.02f + i * 0.33f;
                SetRect(button.GetComponent<RectTransform>(), new Vector2(min, 0.1f), new Vector2(min + 0.3f, 0.9f));
                int index = i;
                button.Button.onClick.AddListener(() => ShowPage(index == 0 ? UiPreviewPage.Home : UiPreviewPage.Gameplay));
            }
        }
    }
}
