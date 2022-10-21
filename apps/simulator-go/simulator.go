package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"math/rand"
	"net/http"
	"strconv"
	"time"
)

type RequestDto struct {
	Message string `json:"message"`
}

const WAIT_INTERVAL = time.Second * 4

func Simulate() {

	go simulate()
}

func simulate() {
	rand.Seed(time.Now().UnixNano())
	for {
		time.Sleep(WAIT_INTERVAL)

		func() {

			requestDto := RequestDto{
				Message: strconv.Itoa(rand.Intn(1000)),
			}

			client := &http.Client{
				Timeout: time.Second * 10,
			}

			requestDtoInBytes, _ := json.Marshal(requestDto)
			request, err := http.NewRequest(http.MethodPost,
				"http://java-first.java.svc.cluster.local:8080/java/second",
				bytes.NewBufferString(string(requestDtoInBytes)),
			)

			if err != nil {
				fmt.Println("Error occurred in creating HTTP request.")
				panic(err)
			}

			request.Header.Set("Content-Type", "application/json")

			response, err := client.Do(request)
			if err != nil {
				fmt.Println("Error occurred in performing HTTP request.")
				panic(err)
			}
			defer response.Body.Close()

			fmt.Println("Create response: " + strconv.Itoa(response.StatusCode))
		}()
	}
}
