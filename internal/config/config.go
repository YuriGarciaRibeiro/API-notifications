package config

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