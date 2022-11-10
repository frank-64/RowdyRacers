using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerCarControl : MonoBehaviour
    {
        [SerializeField] private CarLocomotion carLocomotion;
        public bool driving = false;

        private void FixedUpdate()
        {
            if (driving)
            {
                float verticalInput = Input.GetAxis("Vertical");
                float horizontalInput = Input.GetAxis("Horizontal");
                carLocomotion.Drive(horizontalInput, verticalInput, verticalInput);
            }
        }
    }
}
