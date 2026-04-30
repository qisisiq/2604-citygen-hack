using UnityEngine;

namespace CityGen.Data
{
    [CreateAssetMenu(menuName = "CityGen/Seed Profile", fileName = "CitySeedProfile")]
    public class CitySeedProfile : ScriptableObject
    {
        [SerializeField]
        private CitySeedData data = CitySeedFactory.CreateAscendingWard();

        public CitySeedData Data
        {
            get { return data; }
        }

        private void Reset()
        {
            data = CitySeedFactory.CreateAscendingWard();
        }
    }
}
