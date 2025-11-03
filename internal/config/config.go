package config

import "fmt"

type App struct {
	Name string `mapstructure:"name"`
	Env  string `mapstructure:"env"`
	Port int    `mapstructure:"port"`
}

type Queue struct {
	Type   string       `mapstructure:"type"`
	Rabbit RabbitConfig `mapstructure:"rabbitmq"`
}

type RabbitConfig struct {
	URL          string `mapstructure:"url"`
	Exchange     string `mapstructure:"exchange"`
	ExchangeType string `mapstructure:"exchange_type"`
	DLXExchange  string `mapstructure:"dlx_exchange"`
	MaxRetries   int    `mapstructure:"max_retries"`
	RetryDelay   int    `mapstructure:"retry_delay"` // in milliseconds
}

type WorkerConfig struct {
	Enabled     bool `mapstructure:"enabled"`
	Concurrency int  `mapstructure:"concurrency"`
	RateLimit   int  `mapstructure:"rate_limit"` // per minute
}

type Workers struct {
	Email   WorkerConfig `mapstructure:"email"`
	SMS     WorkerConfig `mapstructure:"sms"`
	Push    WorkerConfig `mapstructure:"push"`
	Webhook WorkerConfig `mapstructure:"webhook"`
}

type ServiceConfig struct {
	Provider string            `mapstructure:"provider"`
	SMTP     *SMTPConfig      `mapstructure:"smtp,omitempty"`
	Twilio   *TwilioConfig    `mapstructure:"twilio,omitempty"`
	Firebase *FirebaseConfig  `mapstructure:"firebase,omitempty"`
}

type SMTPConfig struct {
	Host     string `mapstructure:"host"`
	Port     int    `mapstructure:"port"`
	Username string `mapstructure:"username"`
	Password string `mapstructure:"password"`
	From     string `mapstructure:"from"`
}

type TwilioConfig struct {
	AccountSID string `mapstructure:"account_sid"`
	AuthToken  string `mapstructure:"auth_token"`
	FromNumber string `mapstructure:"from_number"`
}

type FirebaseConfig struct {
	CredentialsFile string `mapstructure:"credentials_file"`
}

type Services struct {
	Email ServiceConfig `mapstructure:"email"`
	SMS   ServiceConfig `mapstructure:"sms"`
	Push  ServiceConfig `mapstructure:"push"`
}

type Database struct {
	Enabled  bool   `mapstructure:"enabled"`
	Driver   string `mapstructure:"driver"`
	Host     string `mapstructure:"host"`
	Port     int    `mapstructure:"port"`
	User     string `mapstructure:"user"`
	Password string `mapstructure:"password"`
	DBName   string `mapstructure:"dbname"`
}

type Logging struct {
	Level  string `mapstructure:"level"`
	Format string `mapstructure:"format"`
}

type Security struct {
	APIKeyEnabled   bool     `mapstructure:"api_key_enabled"`
	JWTEnabled      bool     `mapstructure:"jwt_enabled"`
	AllowedOrigins  []string `mapstructure:"allowed_origins"`
}

type Config struct {
	App      App       `mapstructure:"app"`
	Queue    Queue     `mapstructure:"queue"`
	Workers  Workers   `mapstructure:"workers"`
	Services Services  `mapstructure:"services"`
	Database Database  `mapstructure:"database"`
	Logging  Logging   `mapstructure:"logging"`
	Security Security  `mapstructure:"security"`
}

// Validate valida toda a configuração
func (c *Config) Validate() error {
	if err := c.App.Validate(); err != nil {
		return fmt.Errorf("app config: %w", err)
	}

	if err := c.Queue.Validate(); err != nil {
		return fmt.Errorf("queue config: %w", err)
	}

	if err := c.Workers.Validate(); err != nil {
		return fmt.Errorf("workers config: %w", err)
	}

	if err := c.Logging.Validate(); err != nil {
		return fmt.Errorf("logging config: %w", err)
	}

	return nil
}

// Validate valida a configuração da aplicação
func (a *App) Validate() error {
	if a.Name == "" {
		return fmt.Errorf("app name não pode ser vazio")
	}

	if a.Port <= 0 || a.Port > 65535 {
		return fmt.Errorf("porta inválida: %d (deve estar entre 1 e 65535)", a.Port)
	}

	validEnvs := map[string]bool{
		"development": true,
		"staging":     true,
		"production":  true,
	}

	if !validEnvs[a.Env] {
		return fmt.Errorf("ambiente inválido: %s (use: development, staging ou production)", a.Env)
	}

	return nil
}

// Validate valida a configuração da fila
func (q *Queue) Validate() error {
	if q.Type != "rabbitmq" {
		return fmt.Errorf("tipo de fila inválido: %s (apenas 'rabbitmq' é suportado)", q.Type)
	}

	return q.Rabbit.Validate()
}

// Validate valida a configuração do RabbitMQ
func (r *RabbitConfig) Validate() error {
	if r.URL == "" {
		return fmt.Errorf("RabbitMQ URL não pode ser vazia")
	}

	if r.Exchange == "" {
		return fmt.Errorf("exchange name não pode ser vazio")
	}

	if r.ExchangeType != "topic" && r.ExchangeType != "direct" && r.ExchangeType != "fanout" {
		return fmt.Errorf("exchange type inválido: %s (use: topic, direct ou fanout)", r.ExchangeType)
	}

	if r.MaxRetries < 0 {
		return fmt.Errorf("max retries não pode ser negativo: %d", r.MaxRetries)
	}

	if r.RetryDelay < 0 {
		return fmt.Errorf("retry delay não pode ser negativo: %d", r.RetryDelay)
	}

	return nil
}

// Validate valida a configuração de um worker
func (w *WorkerConfig) Validate() error {
	if w.Concurrency < 0 {
		return fmt.Errorf("concurrency não pode ser negativa: %d", w.Concurrency)
	}

	if w.RateLimit < 0 {
		return fmt.Errorf("rate limit não pode ser negativo: %d", w.RateLimit)
	}

	return nil
}

// Validate valida a configuração de todos os workers
func (w *Workers) Validate() error {
	if err := w.Email.Validate(); err != nil {
		return fmt.Errorf("email worker: %w", err)
	}

	if err := w.SMS.Validate(); err != nil {
		return fmt.Errorf("sms worker: %w", err)
	}

	if err := w.Push.Validate(); err != nil {
		return fmt.Errorf("push worker: %w", err)
	}

	if err := w.Webhook.Validate(); err != nil {
		return fmt.Errorf("webhook worker: %w", err)
	}

	return nil
}

// Validate valida a configuração de logging
func (l *Logging) Validate() error {
	validLevels := map[string]bool{
		"debug": true,
		"info":  true,
		"warn":  true,
		"error": true,
		"fatal": true,
	}

	if !validLevels[l.Level] {
		return fmt.Errorf("log level inválido: %s (use: debug, info, warn, error, fatal)", l.Level)
	}

	validFormats := map[string]bool{
		"json":    true,
		"console": true,
	}

	if !validFormats[l.Format] {
		return fmt.Errorf("log format inválido: %s (use: json ou console)", l.Format)
	}

	return nil
}