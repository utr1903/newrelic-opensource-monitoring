package main

import (
	"github.com/gin-gonic/gin"
	"github.com/prometheus/client_golang/prometheus/promhttp"
)

const PORT string = ":8080"

func main() {
	router := gin.Default()

	// Prometheus metric exposer
	router.GET("/metrics", prometheusHandler())

	// Simulate
	Simulate()

	// Serve
	router.Run(PORT)
}

func prometheusHandler() gin.HandlerFunc {
	h := promhttp.Handler()

	return func(c *gin.Context) {
		h.ServeHTTP(c.Writer, c.Request)
	}
}
