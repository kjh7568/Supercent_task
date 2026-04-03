using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 손님 오브젝트 풀. Get으로 꺼내고 Return으로 돌려준다.
/// </summary>
public class CustomerObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private int initialSize = 10;

    private readonly Queue<Customer> pool = new();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            var obj = Instantiate(customerPrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj.GetComponent<Customer>());
        }
    }

    public Customer Get()
    {
        if (pool.Count > 0)
        {
            var customer = pool.Dequeue();
            customer.gameObject.SetActive(true);
            return customer;
        }

        var newObj = Instantiate(customerPrefab, transform);
        return newObj.GetComponent<Customer>();
    }

    public void Return(Customer customer)
    {
        customer.gameObject.SetActive(false);
        customer.transform.SetParent(transform);
        pool.Enqueue(customer);
        Debug.Log($"[CustomerObjectPool] 손님 반환 (풀 크기: {pool.Count})");
    }
}
