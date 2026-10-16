namespace DentusClinic.API.Enums {
    public enum ResultadoEnvioPushEnum {
        Entregue,
        /// Falha passageira (serviço fora do ar, limite excedido): vale tentar de novo.
        FalhaTemporaria,
        /// O token não serve mais e nunca vai funcionar: o dispositivo deve ser desativado.
        TokenInvalido
    }
}
