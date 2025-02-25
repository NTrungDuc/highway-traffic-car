using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkShape  {
    float x;
    float y;
    float speed;
    float size;
    float lifeTime;

    float tick;

    public bool isLife = false;

    public InkShape(float x,float y,float lifeTime, float size)
    {
        SetParam(x, y, lifeTime,size);
    }

    public void SetParam(float x, float y, float lifeTime,float size)
    {
        this.x = x;
        this.y = y;
        this.size = size;
        speed = Random.value * 5f;

        this.lifeTime = lifeTime;
        tick = 0;
        isLife = true;
    }

    public void Move()
    {
        if (isLife == false)
            return;

        tick += Time.deltaTime;
        if (tick>=lifeTime)
        {
            isLife = false;
            return;
        }

        float sign = 1f; // Random.value > 0.5f ? 1f : -1f;

        x += sign * speed;

        // sign = Random.value > 0.5f ? 1f : -1f;
        y += sign * speed;


        DrawCircle(x, y, size, size, 30, Color.black);
    }

    public void Stop()
    {
        isLife = false;

    }

    public void DrawCircle(float posX, float posY, float radiusX, float radiusY, float step, Color color)
    {
        //GL.Begin(GL.TRIANGLES);

        GL.Color(color);

        for (float i = 0; i < 360f; i += step)
        {
            float r = i * Mathf.PI / 180f;
            float x1 = radiusX * Mathf.Sin(r) + posX;
            float y1 = radiusY * Mathf.Cos(r) + posY;

            GL.Vertex3(posX, posY, 0);
            GL.Vertex3(x1, y1, 0);

            // r = (i + step) * Mathf.PI / 180f;
            // float x2 = radiusX * Mathf.Sin(r) + posX;
            // float y2 = radiusY * Mathf.Cos(r) + posY;
            // GL.Vertex3(x2, y2, 0);

        }

        //GL.End();

    }

}
