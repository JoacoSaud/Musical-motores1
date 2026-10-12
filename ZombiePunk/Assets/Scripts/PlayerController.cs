using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadMovimiento = 5f;
    [SerializeField] private float velocidadRotacion = 2f;
    [SerializeField] private float fuerzaSalto = 5f;
    [SerializeField] private float gravedad = -9.81f;

    [Header("Ataque Cuerpo a Cuerpo")]
    [SerializeField] private float alcanceAtaque = 2f;
    [SerializeField] private float dañoAtaque = 10f;
    [SerializeField] private float tiempoEntreAtaques = 0.5f;
    [SerializeField] private float duracionAtaque = 0.3f;

    [Header("Componentes")]
    [SerializeField] private Camera camaraJugador;
    [SerializeField] private CharacterController controlador;

    private Vector3 velocidad = Vector3.zero;
    private float tiempoUltimoAtaque = -1f;
    private bool estoyAtacando = false;

    private float rotacionX = 0f;
    private float rotacionY = 0f;

    private void Start()
    {
        // Si no asignas los componentes en el inspector, los busca automáticamente
        if (camaraJugador == null)
            camaraJugador = GetComponentInChildren<Camera>();

        if (controlador == null)
            controlador = GetComponent<CharacterController>();

        // Bloquea el cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        ManejadorEntrada();
        ActualizarRotacion();
        ActualizarMovimiento();
    }

    private void ManejadorEntrada()
    {
        // Entrada de movimiento
        float inputHorizontal = Input.GetAxis("Horizontal");
        float inputVertical = Input.GetAxis("Vertical");

        Vector3 direccion = transform.forward * inputVertical + transform.right * inputHorizontal;
        direccion = direccion.normalized;

        if (controlador.isGrounded)
        {
            velocidad.y = 0f;

            // Salto
            if (Input.GetKeyDown(KeyCode.Space))
            {
                velocidad.y = fuerzaSalto;
            }
        }

        velocidad.x = direccion.x * velocidadMovimiento;
        velocidad.z = direccion.z * velocidadMovimiento;

        // Aplicar gravedad
        velocidad.y += gravedad * Time.deltaTime;

        // Ataque cuerpo a cuerpo
        if (Input.GetMouseButtonDown(0))
        {
            AtacarCuerpoACuerpo();
        }

        // Desbloquear cursor con ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    private void ActualizarRotacion()
    {
        // Rotación con el ratón
        float inputRatonX = Input.GetAxis("Mouse X") * velocidadRotacion;
        float inputRatonY = Input.GetAxis("Mouse Y") * velocidadRotacion;

        rotacionY += inputRatonX;
        rotacionX -= inputRatonY;

        // Limitar rotación vertical (para que no gire la cabeza hacia atrás)
        rotacionX = Mathf.Clamp(rotacionX, -90f, 90f);

        // Aplicar rotación
        transform.rotation = Quaternion.Euler(0, rotacionY, 0);
        camaraJugador.transform.localRotation = Quaternion.Euler(rotacionX, 0, 0);
    }

    private void ActualizarMovimiento()
    {
        controlador.Move(velocidad * Time.deltaTime);
    }

    private void AtacarCuerpoACuerpo()
    {
        // Verificar que ha pasado suficiente tiempo desde el último ataque
        if (Time.time - tiempoUltimoAtaque < tiempoEntreAtaques)
            return;

        tiempoUltimoAtaque = Time.time;
        estoyAtacando = true;

        // Raycast desde la cámara hacia adelante
        Ray rayo = new Ray(camaraJugador.transform.position, camaraJugador.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(rayo, out hit, alcanceAtaque))
        {
            // Verificar si el objeto tiene el componente de enemigo
            IDamageable objetivo = hit.collider.GetComponent<IDamageable>();

            if (objetivo != null)
            {
                objetivo.RecibirDaño(dañoAtaque);
                Debug.Log("¡Golpe! Daño: " + dañoAtaque);
            }
        }

        // Opcional: reproducir sonido o animación de ataque
        // AnimarAtaque();
        // ReproducirSonidoAtaque();
    }

    public bool EstaAtacando()
    {
        return Time.time - tiempoUltimoAtaque < duracionAtaque;
    }
}

// Interfaz para que los enemigos implementen la recepción de daño
public interface IDamageable
{
    void RecibirDaño(float cantidad);
}