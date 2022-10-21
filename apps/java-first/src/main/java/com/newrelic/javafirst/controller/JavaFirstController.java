package com.newrelic.javafirst.controller;

import com.newrelic.javafirst.dto.RequestDto;
import com.newrelic.javafirst.dto.ResponseDto;
import com.newrelic.javafirst.service.java.JavaSecondService;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("java")
public class JavaFirstController {

    private final Logger logger = LoggerFactory.getLogger(JavaFirstController.class);

    @Autowired
    private JavaSecondService javaSecondService;

    @PostMapping("second")
    public ResponseEntity<ResponseDto> javaSecondMethod(
            @RequestBody RequestDto requestDto
    ) {
        logger.info("Java second method is triggered...");

        var responseDto = javaSecondService.run(requestDto);

        logger.info("Java second method is executed.");

        return responseDto;
    }
}
