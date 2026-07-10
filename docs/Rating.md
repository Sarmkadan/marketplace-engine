# Rating
The `Rating` type in the `marketplace-engine` project represents a rating system, encapsulating the score, total reviews, and average rating. It provides methods to add reviews and compare ratings, making it a fundamental component for evaluating and comparing items in the marketplace.

## API
* `public int Score`: Gets the current score of the rating.
* `public int TotalReviews`: Gets the total number of reviews.
* `public double AverageRating`: Gets the average rating calculated from the score and total reviews.
* `public Rating()`: Initializes a new instance of the `Rating` class.
* `public Rating AddReview()`: Adds a review to the rating. The method is not fully specified and may require additional parameters in its implementation.
* `public bool Equals(object obj)`: Determines whether the specified object is equal to the current object.
* `public override bool Equals(object obj)`: Determines whether the specified object is equal to the current object, overriding the base class implementation.
* `public override int GetHashCode()`: Serves as the default hash function.
* `public override string ToString()`: Returns a string that represents the current object.

## Usage
The following examples demonstrate how to use the `Rating` class:
```csharp
// Example 1: Creating and using a Rating instance
Rating rating = new Rating();
rating.AddReview(); // Assuming AddReview is implemented to accept a review
Console.WriteLine($"Score: {rating.Score}, Total Reviews: {rating.TotalReviews}, Average Rating: {rating.AverageRating}");

// Example 2: Comparing two Rating instances
Rating rating1 = new Rating();
Rating rating2 = new Rating();
rating1.AddReview();
Console.WriteLine($"Are ratings equal? {rating1.Equals(rating2)}");
```

## Notes
When using the `Rating` class, consider the following:
- The `AddReview` method's implementation details are not provided, so its usage may vary based on the actual implementation.
- The `Equals` method overrides the base class implementation, which may affect comparisons with objects of different types.
- The `GetHashCode` method is overridden, which is crucial for using `Rating` instances in hash-based data structures.
- Thread-safety is not explicitly guaranteed by the provided members, so caution should be exercised when accessing or modifying `Rating` instances in multithreaded environments.
- Edge cases, such as division by zero when calculating the average rating with zero total reviews, should be handled according to the specific requirements of the application.
