using System;
using System.Collections.Generic;
using _Game.Configs;
using _KITSystem.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class BattleUIManager : MonoBehaviour
    {
        public event Action<int> OnElementChanged;
        public event Action OnElementStartReset; 
        public event Action OnElementStopReset; 
        
        private const int MAX = 4;
        [SerializeField] private Transform[] attractorsTarget = new Transform[0];
        [SerializeField] private ElementScroll scroll;
        [SerializeField] private Button btnPush;
        [SerializeField] private Element[] elements = new Element[4];
        [SerializeField] private Image imgHealthFill;
        [SerializeField] private TextMeshProUGUI txtHealth;
        [SerializeField] private Image imgEnergyFill;
        [SerializeField] private float energySpeed = 1;

        private ColorSetting colorSetting;
        private readonly List<int> collections = new List<int>();
        private bool isInitialized = false;
        private float energy;

        private void Awake()
        {
            btnPush.onClick.AddListener(PushOut);
        }

        private void Start()
        {
            collections.Add(0);
            collections.Add(1);
            collections.Add(2);
            collections.Add(3);
        }

        public void Initialize()
        {
            isInitialized = true;
            colorSetting = ColorSetting.Instance;
            FlyAttractor();
        }

        private void Update()
        {
            if (!isInitialized) return;
            
            energy += Time.deltaTime * energySpeed;
            imgEnergyFill.fillAmount = energy;
            if (energy >= 1)
            {
                // todo: play animation energy
                energy = 0;
                imgEnergyFill.fillAmount = 0;
                PushStack();
            }
        }

        private void PushStack()
        {
            collections.Add(RandomUtils.Range(0, MAX));

            for (var i = 0; i < collections.Count; i++)
            {
                colorSetting.TryGetColor(collections[i], out ColorData colorData);
                
                elements[i].Active(colorData.activeColor);
            }
            
            if (collections.Count == MAX)
            {
                FlyAttractor();
            }
        }
        
        private void PushOut()
        {
            if (collections.Count > 0) collections.RemoveAt(collections.Count - 1);
            
            for (var i = 0; i < collections.Count; i++)
            {
                colorSetting.TryGetColor(collections[i], out ColorData colorData);
                elements[i].Active(colorData.activeColor);
            }

            for (int i = collections.Count; i < MAX; i++)
            {
                elements[i].Inactive();
            }
        }

        private void FlyAttractor()
        {
            OnElementStartReset?.Invoke();
            
            Dictionary<int, int> dictCount = new Dictionary<int, int>();
            for (var i = 0; i < collections.Count; i++)
            {
                int id = collections[i];
                if (!dictCount.TryAdd(id, 1)) dictCount[id]++;
            }

            int t = 0;
            for (var i = 0; i < collections.Count; i++)
            {
                t++;
                float delay = 0.1f;
                float smooth = RandomUtils.Range(0.3f, 0.5f);
                float offsetY = 2.5f;
                float radius = RandomUtils.Range(1.1f, 2.0f);
                float duration = 1.2f;
                    
                int id = collections[i];
                int stack = dictCount[id];
                Element element = elements[i];
                Vector3 startPosition = element.transform.position;
                Vector3 endPosition = attractorsTarget[id].transform.position;
                Vector3 rot = new Vector3(0, 0, 45);
                colorSetting.TryGetColor(id, out ColorData colorData);
                element.MoveTo(startPosition, endPosition, rot,
                    stack * delay, duration, radius, offsetY, smooth, () =>
                    {
                        element.Inactive();
                        OnElementChanged?.Invoke(id);
                        scroll.Push(colorData.activeColor);
                        t--;
                        if (t == 0) OnElementStopReset?.Invoke();
                    }
                );
                dictCount[id]--;
            }

            collections.Clear();
        }
    }
}
