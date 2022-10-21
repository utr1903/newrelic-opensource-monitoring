package com.newrelic.javafirst;

import com.newrelic.javafirst.handler.RestTemplateResponseErrorHandler;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.web.client.RestTemplateBuilder;
import org.springframework.context.annotation.Bean;
import org.springframework.web.client.RestTemplate;

@SpringBootApplication
public class JavaFirstApplication {

	public static void main(String[] args) {
		SpringApplication.run(JavaFirstApplication.class, args);
	}

	@Bean
	public RestTemplate createRestTemplate() {
		return new RestTemplateBuilder()
				.errorHandler(new RestTemplateResponseErrorHandler())
				.build();
	}
}
