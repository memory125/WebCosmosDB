package com.wing.azure.cosmos.cosmosdb;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.CommandLineRunner;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

@SpringBootApplication
public class CosmosDbApplication implements CommandLineRunner {
  
   @Autowired
   private UserRepository repository;

   public static void main(String[] args) {
      SpringApplication.run(CosmosDbApplication.class, args);
   }

   public void run(String... var1) throws Exception {
      final User testUser = new User("testId", "testFirstName", "testLastName");

      repository.deleteAll();
      repository.save(testUser);

      final User result = repository.findOne(testUser.getId());

      System.out.printf("\n\n%s\n\n",result.toString());
   }
}
