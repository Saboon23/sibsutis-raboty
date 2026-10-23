using UnityEngine;

public class SpawnerScript : MonoBehaviour
{
    public Transform target;            //цель, к которой должны двигаться монстры
    public GameObject monster;          //ссылка на префаб монстра
    public float spawnTime = 2;         //текущий интервал появления монстров
    public float minimalSpawnTime = 1;  //минимальный интервал появления монстров
    float currentTime = 0;              //время, прошедшее с появления последнего монстра

    void LateUpdate()
    {
        currentTime += Time.deltaTime;  //увеличение времени прошедшего с появления монстра
        if (currentTime > spawnTime)    //если прошло времени больше, чем интервал
        {
            //создание копии монстра из префаба
            GameObject ms_clone = Instantiate(monster, transform.position, Quaternion.identity);
            //получение ссылки на скрипт монстра и установка цели
            MonsterScript ms = ms_clone.GetComponent<MonsterScript>();
            ms.target = target;
            currentTime = 0;    //обнуление счётчика времени появления монстра
            //уменьшение интервала между появлениями монстров
            if (spawnTime > minimalSpawnTime)
                spawnTime -= 0.1f;
            else
                spawnTime = minimalSpawnTime;
        }
    }
}
