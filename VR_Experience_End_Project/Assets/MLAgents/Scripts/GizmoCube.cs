   using UnityEngine;

   public class GizmoCube : MonoBehaviour
   {
       public Color gizmoColor = new Color(1, 0, 0, 0.3f); // Rood, 30% transparant

       void OnDrawGizmos()
       {
           Gizmos.color = gizmoColor;
           Gizmos.DrawCube(transform.position, transform.localScale);
       }
   }