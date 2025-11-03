package main

import (
	"log"

	"github.com/YuriGarciaRibeiro/API-notifications/internal/config"
)

func main() {
	// Carregar configuração
	cfg, err := config.LoadConfig("configs/config.yaml")
	if err != nil {
		log.Fatalf("❌ Erro ao carregar configuração: %v", err)
	}

	// Imprimir configuração carregada (para testar)
	log.Println("✅ Configuração carregada com sucesso!")
	log.Printf("📱 Aplicação: %s", cfg.App.Name)
	log.Printf("🌍 Ambiente: %s", cfg.App.Env)
	log.Printf("🔌 Porta: %d", cfg.App.Port)
	log.Println()
	
	log.Println("📨 Configuração RabbitMQ:")
	log.Printf("  • URL: %s", cfg.Queue.Rabbit.URL)
	log.Printf("  • Exchange: %s", cfg.Queue.Rabbit.Exchange)
	log.Printf("  • Tipo: %s", cfg.Queue.Rabbit.ExchangeType)
	log.Printf("  • Max Retries: %d", cfg.Queue.Rabbit.MaxRetries)
	log.Println()

	log.Println("⚙️  Workers configurados:")
	log.Printf("  • Email: enabled=%v, concurrency=%d, rate_limit=%d/min",
		cfg.Workers.Email.Enabled,
		cfg.Workers.Email.Concurrency,
		cfg.Workers.Email.RateLimit)
	log.Printf("  • SMS: enabled=%v, concurrency=%d, rate_limit=%d/min",
		cfg.Workers.SMS.Enabled,
		cfg.Workers.SMS.Concurrency,
		cfg.Workers.SMS.RateLimit)
	log.Printf("  • Push: enabled=%v, concurrency=%d, rate_limit=%d/min",
		cfg.Workers.Push.Enabled,
		cfg.Workers.Push.Concurrency,
		cfg.Workers.Push.RateLimit)
	log.Println()

	log.Println("📊 Logging:")
	log.Printf("  • Level: %s", cfg.Logging.Level)
	log.Printf("  • Format: %s", cfg.Logging.Format)
	log.Println()

	log.Println("🔒 Segurança:")
	log.Printf("  • API Key: %v", cfg.Security.APIKeyEnabled)
	log.Printf("  • JWT: %v", cfg.Security.JWTEnabled)
	log.Printf("  • CORS Origins: %v", cfg.Security.AllowedOrigins)
	log.Println()

	log.Println("🎉 Sistema pronto para iniciar!")
	// Aqui você vai iniciar o servidor Gin (no Passo 4)
}