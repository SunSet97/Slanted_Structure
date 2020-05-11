using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//클릭 시 총알발사
public class CubeFire : MonoBehaviour
{
    //필요속성 : 총알공장
    public GameObject coinFactory;
    //총구
    public GameObject coinPosition;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //클릭 시 총알 발사
        if (Input.GetButtonDown("Fire1"))
        {
            //총알공장에서 총알 만들기
            GameObject coin = Instantiate(coinFactory);
            //총알을 쏘고싶다 -> 받은 총알 총구에 가져다 놓기
            //bullet의 위치를 cube의 위치로 놓는다.
            coin.transform.position = coinPosition.transform.position;

            //bullet이 향하는 방향을 총구가 향하는 방향으로
            coin.transform.forward = coinPosition.transform.forward;
        }
    }
}
