using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadingOfInkEffect : MonoBehaviour {
    public Material lineMaterial;

    bool isClear = true;

    public float minLifeTime = 1;
    public float maxLifeTime = 3;

    public float minSize = 1;
    public float maxSize = 2;

    List<InkShape> inks = new List<InkShape>();
    List<InkShape> removeInks = new List<InkShape>();

    Vector3 prevMousePosition = Vector3.zero;

	// Use this for initialization
	void Start () {

    }
	

    private void OnRenderObject()
    {
        GL.PushMatrix(); //保存当前Matirx  
        lineMaterial.SetPass(0); //刷新当前材质  
        GL.LoadPixelMatrix();

        if (isClear)
        {
            GL.Clear(true, true, Color.white);
            isClear = false;
        }

        if (Input.GetMouseButton(0))
        {
            if (Vector3.Distance(prevMousePosition, Input.mousePosition) > 0.05f)
            {
                for (var i = 0; i <= Mathf.FloorToInt(Random.value * 15); i++)
                {
                    float lifeTime = Random.Range(minLifeTime, maxLifeTime);
                    float size = Random.Range(minSize, maxSize);
                    //inks.Add(new InkShape(Input.mousePosition.x, Input.mousePosition.y, lifeTime));
                    SetFreeInkShape(Input.mousePosition.x, Input.mousePosition.y, lifeTime,size);
                }

                prevMousePosition = Input.mousePosition;
            }
        }

        if (Input.GetMouseButton(1))
        {
            foreach (InkShape ink in inks)
            {
                ink.Stop();
            }
            isClear = true;
        }

        GL.Begin(GL.TRIANGLES);
        GL.Color(Color.white);
        foreach (InkShape ink in inks)
        {
            ink.Move();
        }
        GL.End();

        GL.PopMatrix();//读取之前的Matrix  

    }

    void SetFreeInkShape(float x,float y,float lifeTime,float size)
    {
        //InkShape ink = null;
        foreach(InkShape ink in inks)
        {
            if (ink.isLife==false)
            {
                ink.SetParam(x, y, lifeTime,size);
                return;
            }
        }

        inks.Add(new InkShape(x, y, lifeTime,size));
    }

}
