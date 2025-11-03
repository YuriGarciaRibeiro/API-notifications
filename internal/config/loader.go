package config

import (
	"fmt"

	"github.com/spf13/viper"
)

func LoadConfig(configPath string) (*Config, error) {
	viper.SetConfigFile(configPath)
	viper.SetConfigType("yaml")

	if err := viper.ReadInConfig(); err != nil {
		return nil, fmt.Errorf("erro ao ler config: %w", err)
	}

	var cfg Config
	if err := viper.Unmarshal(&cfg); err != nil {
		return nil, fmt.Errorf("erro ao fazer unmarshal: %w", err)
	}

	if err := validateConfig(&cfg); err != nil {
		return nil, fmt.Errorf("config inválida: %w", err)
	}

	return &cfg, nil
}

func validateConfig(cfg *Config) error {
	if cfg.App.Port == 0 {
		return fmt.Errorf("porta da aplicação não configurada")
	}

	if cfg.Queue.Rabbit.URL == "" {
		return fmt.Errorf("URL do RabbitMQ não configurada")
	}

	return nil
}

func LoadConfigWithEnv(configPath string) (*Config, error) {
	viper.AutomaticEnv()
	viper.SetEnvPrefix("NOTIF") // Prefixo para env vars: NOTIF_APP_PORT

	viper.BindEnv("app.port", "NOTIF_APP_PORT")
	viper.BindEnv("queue.rabbitmq.url", "RABBITMQ_URL")
	
	return LoadConfig(configPath)
}