# # def solve_fruit_problem():
# #     # 1. 사전 확률 (Prior): 각 그릇을 선택할 확률
# #     p_h1 = 0.5  # 그릇 #1 선택
# #     p_h2 = 0.5  # 그릇 #2 선택

# #     # 2. 조건부 확률 (Likelihood): 그릇이 정해졌을 때 바나나를 뽑을 확률
# #     # 그릇 #1: 사과 10, 바나나 30 (총 40개)
# #     p_e_given_h1 = 30 / 40  
# #     # 그릇 #2: 사과 20, 바나나 20 (총 40개)
# #     p_e_given_h2 = 20 / 40  

# #     # 3. 베이즈 정리 공식 적용 (Posterior)
# #     # P(H1|E) = (P(E|H1) * P(H1)) / (P(E|H1) * P(H1) + P(E|H2) * P(H2))
# #     numerator = p_e_given_h1 * p_h1
# #     denominator = (p_e_given_h1 * p_h1) + (p_e_given_h2 * p_h2)
    
# #     p_h1_given_e = numerator / denominator

# #     print(f"바나나를 뽑았을 때, 그릇 #1에서 왔을 확률: {p_h1_given_e:.2f}")

# # solve_fruit_problem()

# # total = int(input("총 그릇은 몇개입니까?"))
# # a=1/total
# # b=1/total
# # c=0.75
# # d=0.5

# # p=(c*a)/((c*a)+(d*b))
# # print(p)

# # import random

# # def simulate_fruit_pick(trials=100000):
# #     # 그릇 정의 (B: 바나나, A: 사과)
# #     bowl1 = ['B'] * 30 + ['A'] * 10
# #     bowl2 = ['B'] * 20 + ['A'] * 20
    
# #     banana_from_bowl1 = 0
# #     total_bananas = 0

# #     for _ in range(trials):
# #         # 1. 무작위로 그릇 선택
# #         chosen_bowl = random.choice([bowl1, bowl2])
        
# #         # 2. 선택된 그릇에서 무작위로 과일 선택
# #         pick = random.choice(chosen_bowl)
        
# #         # 3. 만약 뽑은 과일이 바나나인 경우만 카운트 (증거 E)
# #         if pick == 'B':
# #             total_bananas += 1
# #             # 그 바나나가 그릇 #1에서 나온 경우
# #             if chosen_bowl == bowl1:
# #                 banana_from_bowl1 += 1

# #     # 결과 계산: P(H1|E) = (그릇1에서 나온 바나나 수) / (전체 바나나 수)
# #     probability = banana_from_bowl1 / total_bananas
# #     print(f"시뮬레이션 결과 ({trials}회): {probability:.4f}")

# # simulate_fruit_pick()

# # 문제1 바나나를 뽑았는데 그것이 그릇1일 확률
# a=0.5
# b=0.5
# c=0.75
# d=0.5

# p=(c*a)/((c*a)+(d*b))
# print(p)

# #문제2 바나나를 뽑았는데 그것이 그릇2일 확률
# a=0.5
# b=0.5
# c=0.75
# d=0.5

# p=(d*b)/((c*a)+(d*b))
# print(p)

# #문제3 사과를 뽑았는데 그것이 첫번째 그릇일 확률
# a=0.5
# b=0.5
# c=0.25
# d=0.5

# p=(c*a)/((c*a)+(d*b))
# print(p)

# #문제4 사과를 뽑았는데 그것이 두번째 그릇일 확률
# a=0.5
# b=0.5
# c=0.25
# d=0.5

# p=(d*b)/((c*a)+(d*b))
# print(p)

#과제
a=0.95
b=0.01
c=0.99
d=0.02

z=(a*b)/((a*b)+(d*c))
print(z)
