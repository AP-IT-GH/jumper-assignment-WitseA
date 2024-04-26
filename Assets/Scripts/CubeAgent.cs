using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Collections.Generic;

public class CubeAgent : Agent
{
    public GameObject spherePrefab; // Reference to the sphere prefab
    public GameObject spawnPointsParent;
    private Transform[] spawnPoints;
    private List<GameObject> obstacles = new List<GameObject>();

    private float timer = 0;

    private GameObject spawnedEnemy;

    public override void OnEpisodeBegin()
    {
        spawnPoints = spawnPointsParent.GetComponentsInChildren<Transform>();
        this.transform.localPosition = new Vector3(0, 0.5f, 0);
        this.transform.localRotation = Quaternion.identity;
        float random = Random.Range(0.2f, 0.6f);
        foreach (Transform spawnPoint in spawnPoints)
        {
            spawnedEnemy = Instantiate(spherePrefab, spawnPoint.position, Quaternion.identity);
            obstacles.Add(spawnedEnemy);

            MoveSphere moveSphere = spawnedEnemy.GetComponent<MoveSphere>();
            if (moveSphere != null)
            {
                moveSphere.speed = random;
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.localPosition);
        if (spawnedEnemy != null)
        {
        sensor.AddObservation(spawnedEnemy.transform.localPosition);

        }
    }

    public float speedMultiplier = 0.1f;
    public float jumpMultiplier = 1f;
    private bool hasJumped = false;
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Acties, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        controlSignal.y = actions.ContinuousActions[2];
        transform.Translate(new Vector3(controlSignal.x * speedMultiplier, controlSignal.y * jumpMultiplier, controlSignal.z * speedMultiplier));

        timer++;
        if (timer > 1000f)
        {
            float random = Random.Range(0.2f, 0.6f);
            foreach (Transform spawnPoint in spawnPoints)
            {
                spawnedEnemy = Instantiate(spherePrefab, spawnPoint.position, Quaternion.identity);
                obstacles.Add(spawnedEnemy);
                
                MoveSphere moveSphere = spawnedEnemy.GetComponent<MoveSphere>();
                if (moveSphere != null)
                {
                    moveSphere.speed = random;
                }
            }
            timer = 0;
        }

        // Beloningen

        if (this.transform.position.y >= 1)
        {
            hasJumped = true;
            SetReward(0.001f);
        } else
        {
            hasJumped = false;
        }

        if (this.spawnedEnemy != null)
        {
            if (hasJumped && this.transform.position.x > this.spawnedEnemy.transform.position.x)
            {
                SetReward(1f);
            }
        }
        
        // Van het plaform gevallen?
        if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
            if (obstacles.Count != 0)
            {
                foreach (GameObject obj in obstacles)
                {
                    Destroy(obj);
                }
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continiousActionsOut = actionsOut.ContinuousActions;
        continiousActionsOut[0] = Input.GetAxis("Vertical");
        continiousActionsOut[1] = Input.GetAxis("Horizontal");
        continiousActionsOut[2] = Input.GetKey(KeyCode.Space) ? 1f : 0;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag.Equals("Obstacle"))
        {
            SetReward(-0.5f);
            if(obstacles.Count != 0)
            {
                foreach (GameObject obj in obstacles)
                {
                    Destroy(obj);
                }
            }
            Debug.Log("You STUPID!!!!");
        }
    }
}
