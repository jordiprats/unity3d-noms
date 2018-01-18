using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class draganddrop : MonoBehaviour {

    public AudioClip sound_ok;
    public AudioClip sound_error;

    private bool draggingItem = false;
    private GameObject draggedObject;
    private Vector2 touchOffset;

    private Dictionary<string, float> opcio_reset_pos;

    //elements joc
    public GameObject[] ops;
    public GameObject nomsecret;

    //recursos
    public Object[] fotos;
    public Object[] nomssecrets;

    //ordre
    public int[] llista_alumnes;

    System.Random rnd = new System.Random();

    public void Start()
    {

        ops = new GameObject[3];

        fotos = Resources.LoadAll("fotos", typeof(Sprite));
        nomssecrets = Resources.LoadAll("nomsecret", typeof(Sprite));

        Debug.Log("fotos: " + fotos.Length);
        Debug.Log("nomssecrets: " + nomssecrets.Length);

        this.llista_alumnes = new int[fotos.Length];

        for (int i = 0; i < this.llista_alumnes.Length; i++)
        {
            this.llista_alumnes[i] = i;
        }

        //desordenar
        llista_alumnes = Enumerable.Range(0, this.llista_alumnes.Length).OrderBy(r => rnd.Next()).ToArray();

        Debug.Log(llista_alumnes[0]);

        ops[0] = GameObject.Find("opcio0");
        ops[1] = GameObject.Find("opcio1");
        ops[2] = GameObject.Find("opcio2");

        nomsecret = GameObject.Find("nomsecret");

        //asignar fotos inicials
        for (int i = 0; i < 3; i++)
        {
            Debug.Log("opcio " + i + ": " + this.llista_alumnes[i]);
            ops[i].GetComponent<SpriteRenderer>().sprite = fotos[this.llista_alumnes[i]] as Sprite;
        }

        //actualitzar collider opcions
        for (int i = 0; i < 3; i++)
        {
            Vector2 S = ops[i].GetComponent<SpriteRenderer>().sprite.bounds.size;
            ops[i].GetComponent<BoxCollider2D>().size = S * 1.8f;
            ops[i].GetComponent<BoxCollider2D>().offset = new Vector2((S.x / 2), 0);
        }

        //nom secret inicial
        int random_index = (int)Random.Range(0f, 3f);

        for (int i = 0; i < nomssecrets.Length; i++)
        {
            Sprite item = nomssecrets[i] as Sprite;
            //Debug.Log("busca: " + ops[random_index].GetComponent<SpriteRenderer>().sprite.name + " vs " + item.name);
            if (ops[random_index].GetComponent<SpriteRenderer>().sprite.name == item.name)
            {
                nomsecret.GetComponent<SpriteRenderer>().sprite = item;
            }
            /* else
            {
                Debug.Log("no trobat - aixo no hauria de pasar");
                nomsecret.GetComponent<SpriteRenderer>().sprite = item;
            } */
        }

        //actualitzar collider nomsecret
        Vector2 v2 = nomsecret.GetComponent<SpriteRenderer>().sprite.bounds.size;
        nomsecret.GetComponent<BoxCollider2D>().size = v2 * 1.8f;
        nomsecret.GetComponent<BoxCollider2D>().offset = new Vector2((v2.x / 2), 0);

        Debug.Log("op1 /" + ops[0].GetComponent<SpriteRenderer>().sprite.name + "/");
        Debug.Log("op2 /" + ops[1].GetComponent<SpriteRenderer>().sprite.name + "/");
        Debug.Log("op3 /" + ops[2].GetComponent<SpriteRenderer>().sprite.name + "/");
        Debug.Log("nomsecret /" + nomsecret.GetComponent<SpriteRenderer>().sprite.name + "/");

        opcio_reset_pos = new Dictionary<string, float>();

        opcio_reset_pos.Add("opcio0", 0f);
        opcio_reset_pos.Add("opcio2", 3f);
        opcio_reset_pos.Add("opcio1", -3f);
    }

    void DropItem()
    {
        if(draggingItem)
        {
            draggingItem = false;
            draggedObject.transform.localScale = new Vector3(1.8f, 1.8f, 1.8f);
        }
    }

    Vector2 CurrentTouchPosition
    {
        get
        {
            Vector2 inputPos;
            inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return inputPos;
        }
    }

    // Update is called once per frame
    void Update () {
        if (HasInput)
        {
            DragOrPickUp();
        }
        else
        {
            if (draggingItem)
                DropItem();
        }
    }

    private void DragOrPickUp()
    {
        var inputPosition = CurrentTouchPosition;
        AudioSource audio = GetComponent<AudioSource>();

        if (draggingItem)
        {
            draggedObject.transform.position = inputPosition + touchOffset;
            RaycastHit2D[] touches = Physics2D.RaycastAll(inputPosition, inputPosition, 0.5f);
            if (touches.Length > 1)
            {
                //Debug.Log(" - ini -");
                GameObject nomsecret = null;
                GameObject opcio = null;
                for (int i=0;i<touches.Length;i++)
                {
                    //Debug.Log(touches[i].transform.gameObject.name);
                    if(touches[i].transform.gameObject.name=="nomsecret")
                    {
                        nomsecret = touches[i].transform.gameObject;
                    }
                    if (touches[i].transform.gameObject.name.Contains("opcio"))
                    {
                        //Debug.Log("opcio trobada: " + touches[i].transform.gameObject.name);
                        opcio = touches[i].transform.gameObject;
                    }

                    if(nomsecret != null && opcio != null)
                    {
                        //Debug.Log("touche "+opcio.GetComponent<SpriteRenderer>().sprite.name);
                        if(opcio.GetComponent<SpriteRenderer>().sprite.name == nomsecret.GetComponent<SpriteRenderer>().sprite.name)
                        {
                            audio.clip = sound_ok;

                            audio.Play();
                            Debug.Log("got it: " + opcio.GetComponent<SpriteRenderer>().sprite.name + " vs "+ nomsecret.GetComponent<SpriteRenderer>().sprite.name);
                            //reset pos
                            opcio.transform.position = new Vector3(-7f, opcio_reset_pos[opcio.name], 0f); //null exception here
                            if (draggingItem)
                                DropItem();

                            int nou_nen_index = (int)Random.Range(0f, fotos.Length-1);

                            string nen_antic = opcio.GetComponent<SpriteRenderer>().sprite.name;
                            //canvio nen
                            opcio.GetComponent<SpriteRenderer>().sprite = fotos[nou_nen_index] as Sprite;
                            Debug.Log("canvi " + nen_antic + " per " + opcio.GetComponent<SpriteRenderer>().sprite.name);

                            //canvio nom secret per un random dels 3 possibles
                            int random_index = (int)Random.Range(0f, fotos.Length - 1) % 3;

                            for (int j = 0; j < nomssecrets.Length; j++)
                            {
                                Sprite item = nomssecrets[j] as Sprite;
                                //Debug.Log("busca: " + ops[random_index].GetComponent<SpriteRenderer>().sprite.name + " vs " + item.name);
                                if (ops[random_index].GetComponent<SpriteRenderer>().sprite.name == item.name)
                                {
                                    Debug.Log("nou: " + item.name);
                                    nomsecret.GetComponent<SpriteRenderer>().sprite = item;

                                    for(int x=0;x<3;x++)
                                    {
                                        Debug.Log("possibles: " + ops[x].GetComponent<SpriteRenderer>().sprite.name);
                                    }
                                }
                            }
                        }
                        else
                        {
                            audio.clip = sound_error;

                            audio.Play();
                            //reset pos
                            opcio.transform.position= new Vector3(-7f, opcio_reset_pos[opcio.name], 0f);
                            if(draggingItem)
                                DropItem();
                        }
                    }
                }
                //Debug.Log(" - fi -");
            }
        }
        else
        {
            RaycastHit2D[] touches = Physics2D.RaycastAll(inputPosition, inputPosition, 0.5f);
            if (touches.Length > 0)
            {
                //Debug.Log("touche!");

                var hit = touches[0];
                if (hit.transform != null)
                {
                    if(hit.transform.gameObject.name.Contains("opcio"))
                    {
                        draggingItem = true;
                        draggedObject = hit.transform.gameObject;
                        touchOffset = (Vector2)hit.transform.position - inputPosition;
                        draggedObject.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
                    }
                }
            }
        }
    }

    private bool HasInput
    {
        get
        {
            // returns true if either the mouse button is down or at least one touch is felt on the screen
            return Input.GetMouseButton(0);
        }
    }
}
