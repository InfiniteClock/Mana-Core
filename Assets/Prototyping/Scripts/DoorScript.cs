using UnityEngine;

public class DoorScript : Enemy
{
    public GameObject Door;


    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (!targetDetector.spottedTarget || targetDetector.isConfused)
        {
            //Debug.Log(dState);
            Door.gameObject.SetActive(false);
        } 
        else
        {
            //Debug.Log(dState);
            Door.gameObject.SetActive(true);
        }
    }

    
        
    
}
