# Jumper Exercise

## Stap 1:

### Maak een ML Agent aan en zorg voor het een juiste configuratie.

Maak een ML Agent aan en geef deze het volgende script mee:

```cs
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Collections.Generic;

public class CubeAgent : Agent
{
    public GameObject spherePrefab;
    public GameObject spawnPointsParent;
    private Transform[] spawnPoints;

    private float timer = 0;
```

De methode OnEpisodeBegin runt aan het begin van elke episode. Deze methode voegt functionalteit toe om obstakels te genereren.

```cs
    public override void OnEpisodeBegin()
    {
        spawnPoints = spawnPointsParent.GetComponentsInChildren<Transform>();
        if (this.transform.localPosition.y < 0)
        {
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
            this.transform.localRotation = Quaternion.identity;
        }

        if (spherePrefab != null && spawnPoints != null && spawnPoints.Length > 0)
        {
            foreach (Transform spawnPoint in spawnPoints)
            {
                Instantiate(spherePrefab, spawnPoint.position, Quaternion.identity);
            }
        }

    }
```

De methode CollectObservations gaat aan de Agent verschillende parameters geven die de hem zou kunnen helpen met het vinden en voltooien van zijn doel. We hebben hier de Agent zijn eigen locatie meegegeven.

```cs
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.localPosition);
    }
```

De methode OnActionReceived voert code uit telkens wanneer de Agent een actie uitvoert, zoals naar links bewegen.

In dit voorbeeld gebruiken we een timer die om de 2000 frames een nieuwe rij obstakels genereert. Als de y-coördinaat van de Agent kleiner is dan 0 (als de Agent van het platform valt), wordt de episode beëindigd.

```cs
    public float speedMultiplier = 0.1f;
    public float jumpMultiplier = 1f;
    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        controlSignal.y = actions.ContinuousActions[2];
        transform.Translate(new Vector3(controlSignal.x * speedMultiplier, controlSignal.y * 1, controlSignal.z * speedMultiplier));

        timer++;
        if (timer > 2000f)
        {
            foreach (Transform spawnPoint in spawnPoints)
            {
                Instantiate(spherePrefab, spawnPoint.position, Quaternion.identity);
            }
            timer = 0;
        }

        if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }
```

De methode Heuristic definieert de verschillende acties die de Agent kan uitvoeren. Deze methode in combinatie met de gedragspatronen van de Agent de zetten op "Heuristic", zorgt ervoor dat je zelf de agent kan gaan besturen om bv. de verschillende functionaliteiten te testen voor je gaat beginnen trainen.

```cs
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
            EndEpisode();
        }
    }
}
```

## Stap 2:

### Zorg voor obstakels in de scène

Maak een obstakel aan in de scène en geef dit een bepaald materiaal om hem een kleur te geven. Geef hierbij ook het MoveSphere script mee.  
Dan sleep je het obstakel naar de Assets folder waardoor deze een prefab wordt. Deze prefab kan je dan meegeven aan het CubeAgent script om de obstakels te kunnnen inspawnen.

Dit script zorgt ervoor dat het obstakel beweegt in de -x richting aan de hand van een meegegeven snelheid.

- _Obstakel laten bewegen:_

```cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSphere : MonoBehaviour
{
    public float speed;

    void Start()
    {

    }

    void Update()
    {
        transform.position += new Vector3(-speed, 0, 0);

        if (transform.position.x < -20)
        {
            Destroy(this.gameObject);
        }
    }
}
```

Dit script spawnt een rij obstakels op vooraf gedefinieerde spawnpunten in de scène. Dankzij het MoveSphere script op de obstakels, bewegen deze direct na het spawnen.

- _Obstakel laten spawnen:_

```cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public GameObject spherePrefab;
    public List<Transform> spawnPoints;

    void Start()
    {
        if (spherePrefab != null && spawnPoints != null && spawnPoints.Count > 0)
        {
            foreach (Transform spawnPoint in spawnPoints)
            {
                Instantiate(spherePrefab, spawnPoint.position, Quaternion.identity);
            }
        }
    }

    void Update()
    {
        SpawnSpheresWithDelay();
    }

    private IEnumerator SpawnSpheresWithDelay()
    {
        yield return new WaitForSeconds(10f);
        foreach (Transform spawnPoint in spawnPoints)
        {
            MoveSphere moveSphere = spawnpoint.GetComponent<MoveSphere>();
            if(moveSphere != null){
                moveSphere.speed = Random.Range(0.01f, 0.09f)
            }
            Instantiate(spherePrefab, spawnPoint.position, Quaternion.identity);
        }
        yield return new WaitForSeconds(10f);
    }
}
```

## Stap 3:

### Geef een beloning aan de Agent als hij over de obstakels springt:

Voeg dit deel toe aan het CubeAgent script. Onder de timer die de obstakels spawnt.

Dit script gaat checken of de Agent zijn y-coördinaat boven de 1 is. Dit is hoger dan de hoogte van het obstakel. Als dit waar is dan springt de Agent en gaat een boolean "hasJumped" die je voor de methode moet initialiseren op waar gezet worden. Anders gaat deze op onwaar gezet worden.

Dan wordt een check gedaan of de Agent heeft gesprongen en dat zijn x-coördinaat hoger is dan die van de obstakels. Als dit zo is dan betekent het dat de Agent over de obstacles is gesprongen.

```cs
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
```

## Stap 4:

### Figuur vanuit tensorboard

![afbeelding](https://github.com/ArnoMassart/Jumper/assets/74812715/ab96c55e-fb75-48ff-afae-07b63c7e5c9d)

## Stap 5:

### Hoe de training verliep

Het trainingsproces heeft een punt bereikt waarbij de prestaties van de Agent consistent verbeterden over tijd. Dit heeft ons het idee gegeven dat de Agent goed heeft kunnen leren over de verschillende iteraties van zijn trainingen.

De Agent bleef niet altijd consistent, zeker in het begin ging hij vooral van het platform af en het duurde even voordat hij effectief richting het obstakel ging en er ook eveneens over springen.

Evaluatiemetrieken zoals beloningsaccumulatie, slagingspercentage of taakvoltooiingstijd lieten consequent positieve trends zien, wat de leervoortgang en effectiviteit van de agent weergeeft.

##### _Door Arno Massart en Witse Audenaert_
