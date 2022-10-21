package com.newrelic.javafirst.service.java;

import com.newrelic.javafirst.dto.RequestDto;
import com.newrelic.javafirst.dto.ResponseDto;
import io.opentelemetry.instrumentation.annotations.WithSpan;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.*;
import org.springframework.stereotype.Service;
import org.springframework.web.client.RestTemplate;

import java.util.Collections;

@Service
public class JavaSecondService {

    private final Logger logger = LoggerFactory.getLogger(JavaSecondService.class);

    @Autowired
    private RestTemplate restTemplate;

    public JavaSecondService() {}

    @WithSpan
    public ResponseEntity<ResponseDto> run(
            RequestDto requestDto
    ) {
        logger.info("Making request to java second service...");

        var response = makeRequestToJavaSecondService(requestDto);

        logger.info("Request to java second service is performed.");
        return response;
    }

    @WithSpan
    private ResponseEntity<ResponseDto> makeRequestToJavaSecondService(
            RequestDto requestDto
    ) {
        var url = "http://java-second.java.svc.cluster.local:8080/java";

        var headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);
        headers.setAccept(Collections.singletonList(MediaType.APPLICATION_JSON));

        var entity = new HttpEntity<>(requestDto, headers);
        return restTemplate.exchange(url, HttpMethod.POST, entity, ResponseDto.class);
    }
}
