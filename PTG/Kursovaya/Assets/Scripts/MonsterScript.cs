using UnityEngine;
using UnityEngine.AI;

public class MonsterScript : MonoBehaviour
{
    public Transform target;            //позиция объекта, к которому будет бежать монстр
    NavMeshAgent agent;                 //ссылка на объект навигации
    Animator anim;                      //ссылка на аниматор
    public float attack_distance = 5;   //дистанция атаки
    bool dead = false;                  //монстр уже убит

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();       //получение ссылки на объект навигации
        anim = GetComponent<Animator>();            //получение ссылки на аниматор
        agent.SetDestination(target.position);      //установка цели, к которой будет двигаться монстр
        agent.stoppingDistance = attack_distance;   //на каком расстоянии от цели остановится монстр
    }

    void LateUpdate()
    {
        if (dead) return;

        //вычисление дистанции между противником и целью
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= attack_distance)    //если монстр подошёл на дистанцию атаки
        {
            anim.SetBool("Attack", true);   //запустить анимацию атаки
        }
    }

    public void Death()
    {
        if (dead) return;                           //повторный клик по трупу не считается
        dead = true;
        anim.SetBool("Die", true);                  //анимация смерти монстра
        agent.SetDestination(transform.position);   //остановка монстра

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;       //чтобы луч больше не попадал в монстра

        if (ScoreManager.instance != null)
            ScoreManager.instance.AddScore();       //начисление очков за убитого монстра
    }

    public void Die()
    {
        Destroy(this.gameObject);   //уничтожение объекта
    }
}
