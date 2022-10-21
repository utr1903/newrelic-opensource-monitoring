# Build
FROM maven:3.6.0-jdk-11-slim AS build
COPY src /home/app/src
COPY pom.xml /home/app
RUN mvn -f /home/app/pom.xml clean package

# Package stage
FROM openjdk:11-jre-slim
COPY --from=build /home/app/target/java-second-0.0.1-SNAPSHOT.jar /usr/local/lib/java-second.jar

ADD opentelemetry/opentelemetry.jar /usr/local/lib

EXPOSE 8080

ENTRYPOINT ["java","-javaagent:/usr/local/lib/opentelemetry.jar","-jar","/usr/local/lib/java-second.jar"]
