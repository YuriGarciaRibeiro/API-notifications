# Design: Diagnóstico de Maturidade + Plano de Confiabilidade (0-30 dias)

Data: 2026-05-07  
Projeto: API-notifications  
Objetivo principal: aumentar confiabilidade de produção em 30 dias, sem perder direção de produto e visão executiva.

## 1. Contexto e objetivo

Este design define como avaliar a maturidade do projeto em três perspectivas integradas:

1. Engenharia de software
2. Produto/integração
3. Visão executiva para decisão

A prioridade explícita é confiabilidade de produção no horizonte de 0 a 30 dias.

## 2. Escopo

Incluído:

1. Avaliação de maturidade por pilares técnicos e de entrega
2. Tradução do risco técnico para impacto de produto
3. Priorização executiva em P0/P1/P2 com foco em redução de incidentes
4. Plano operacional semanal de 30 dias
5. KPIs para acompanhar estabilização

Excluído:

1. Replatforming completo
2. Refatorações estruturais sem impacto direto em confiabilidade
3. Iniciativas de longo prazo (>30 dias), exceto como risco residual

## 3. Método de avaliação

### 3.1 Pilares

1. Arquitetura e código
2. Confiabilidade operacional
3. Qualidade e testes
4. Segurança operacional
5. Entrega e documentação
6. Produto e experiência de integração

### 3.2 Escala de maturidade (1-5)

1. Ad hoc
2. Básico
3. Funcional
4. Consistente
5. Otimizado

Cada nota deve ter:

1. Evidência concreta no repositório
2. Risco associado
3. Impacto em produção
4. Ação recomendada

### 3.3 Modelo de criticidade

Criticidade = Probabilidade x Impacto, com classificação:

1. Crítico (P0)
2. Alto (P1)
3. Moderado (P2)

## 4. Estrutura do diagnóstico (3 camadas)

### 4.1 Camada Engenharia

Mapeia falhas e fragilidades que elevam risco operacional:

1. Disponibilidade da API e dos consumers
2. Robustez de filas, retry e DLQ
3. Integridade de contrato HTTP
4. Segurança de configuração e superfície exposta
5. Observabilidade, detecção e resposta

### 4.2 Camada Produto

Traduz riscos técnicos para impacto de negócio:

1. Quebra de integração frontend/parceiros
2. Atraso de entrega por retrabalho
3. Queda de confiança no serviço
4. Aumento de esforço de suporte

### 4.3 Camada Executiva

Converte diagnóstico em decisão:

1. Ordem de execução (P0/P1/P2)
2. Estimativa de esforço (baixo/médio/alto)
3. Dependências e bloqueadores
4. Ganho esperado por ação

## 5. Abordagens consideradas

### Abordagem A (recomendada): risco operacional primeiro

Prós:

1. Reduz mais rápido a chance de incidente
2. Gera ganhos visíveis em 30 dias
3. Preserva foco e evita dispersão

Contras:

1. Menor profundidade estratégica de longo prazo no primeiro ciclo

### Abordagem B: balanceada entre pilares

Prós:

1. Visão mais ampla desde o início

Contras:

1. Diminui velocidade de mitigação de risco crítico

### Abordagem C: benchmark formal por nível

Prós:

1. Comunicação executiva simples

Contras:

1. Risco de relatório sem impacto operacional imediato

Decisão: adotar Abordagem A, sem perder leitura de produto e visão executiva no relatório final.

## 6. Plano de execução (0-30 dias)

### Semana 1 (P0): blindagem mínima e baseline

1. Definir baseline operacional (status atual + principais riscos)
2. Implementar checks mínimos de prontidão/liveness
3. Fechar lacunas de segurança operacional de alto risco
4. Publicar runbook curto de incidente

Critério de saída:

1. Existe forma padronizada de saber se API e workers estão saudáveis
2. Existe resposta operacional para incidentes comuns

### Semana 2 (P0/P1): confiabilidade assíncrona

1. Hardening do fluxo RabbitMQ (consumo, falha permanente, DLQ)
2. Melhorar telemetria de erro por canal
3. Definir alertas essenciais

Critério de saída:

1. Falha de processamento é detectada rapidamente
2. Fluxo de recuperação está claro (reprocesso DLQ e status)

### Semana 3 (P1): previsibilidade de mudança

1. Pipeline mínima de CI para build + smoke
2. Primeira suíte de testes críticos (contrato e regressão principal)
3. Gate mínimo antes de merge/release

Critério de saída:

1. Mudança crítica não entra sem validação mínima
2. Regressões de contrato mais prováveis são barradas cedo

### Semana 4 (P1/P2): estabilização e governança leve

1. Checklist de release orientado a confiabilidade
2. Revisão de riscos residuais
3. Ajuste de backlog para próximo ciclo

Critério de saída:

1. Fluxo de entrega mais previsível
2. Riscos fora de escopo registrados com dono e data-alvo

## 7. KPIs de estabilização

1. SLO de disponibilidade da API
2. Taxa de falha por canal (email/sms/push/bulk)
3. Tempo médio de detecção (MTTD)
4. Tempo médio de recuperação (MTTR)
5. Taxa de regressão por release
6. Percentual de mudanças passando em validações mínimas antes de merge

## 8. Entregáveis

1. Relatório executivo de maturidade (3 camadas)
2. Scorecard por pilar (1-5) com evidências
3. Mapa de risco com priorização P0/P1/P2
4. Backlog de 30 dias com ordem semanal
5. Matriz de KPIs para acompanhamento

## 9. Critérios de sucesso do ciclo de 30 dias

1. Redução clara do risco operacional crítico
2. Maior previsibilidade de deploy e mudanças
3. Menos retrabalho por inconsistência de contrato
4. Melhora mensurável em pelo menos 3 KPIs definidos

## 10. Riscos e mitigação

1. Escopo inflar para além de confiabilidade
Mitigação: congelar foco em ações que reduzem risco de produção no ciclo atual.

2. Sobrecarga de execução sem evidência
Mitigação: toda ação deve ter critério objetivo de validação.

3. Dependência de mudanças maiores
Mitigação: registrar como risco residual e quebrar em incrementos menores.

## 11. Próxima etapa

Após revisão e aprovação deste design, a próxima etapa é escrever o plano detalhado de implementação com o skill `superpowers:writing-plans`.
