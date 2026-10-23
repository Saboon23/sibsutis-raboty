using UnityEngine;

public class MouseScript : MonoBehaviour
{
    public Camera cam;          //ссылка на камеру
    public LayerMask monsters;  //слой, на котором будут искаться пересечения

    void LateUpdate()
    {
        RaycastHit hit;     //переменная под результат пересечения
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);    //построение луча через курсор мыши
        if (Input.GetMouseButtonDown(0))    //если нажата левая кнопка мыши
            if (Physics.Raycast(ray, out hit, 1000, monsters))      //если найдено пересечение
            {
                //попытка получить ссылку на скрипт монстра
                MonsterScript ms = hit.transform.GetComponent<MonsterScript>();
                if (ms == null)
                    ms = hit.transform.GetComponentInParent<MonsterScript>();
                if (ms != null) //если ссылка получена
                {
                    ms.Death(); //вызов функции "Смерть" монстра
                }
            }
    }
}
